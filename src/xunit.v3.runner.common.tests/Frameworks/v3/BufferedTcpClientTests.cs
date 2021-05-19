using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Runner.v3;

public class BufferedTcpClientTests
{
	[Fact(Skip = "Flaky")]
	public async ValueTask MessageSentByClientIsReceivedByServer()
	{
		var server = new Server();
		var port = server.Start();
		var client = new Client(port);
		await client.Start();

		client.Send("Test123\n");
		await client.DisposeAsync();

		var deadline = DateTimeOffset.Now.AddSeconds(30);
		while (server.Requests.Count == 0)
		{
			if (deadline < DateTimeOffset.Now)
				Assert.Fail("Timed out waiting for responses");
			await Task.Delay(50);
		}

		await server.DisposeAsync();

		var msg = Assert.Single(server.Requests);
		Assert.Equal("Test123", msg);
	}

	[Fact(Skip = "Flaky")]
	public async ValueTask MessageSentByServerIsReceivedByClient()
	{
		var server = new Server();
		var port = server.Start();
		var client = new Client(port);
		await client.Start();

		server.Send("Test123\n");
		await server.DisposeAsync();

		var deadline = DateTimeOffset.Now.AddSeconds(30);
		while (client.Requests.Count == 0)
		{
			if (deadline < DateTimeOffset.Now)
				Assert.Fail("Timed out waiting for responses");
			await Task.Delay(50);
		}

		await client.DisposeAsync();

		var msg = Assert.Single(client.Requests);
		Assert.Equal("Test123", msg);
	}

	[Fact(Skip = "Flaky")]
	public async ValueTask MessagesAreRetrievedInOrder()
	{
		var server = new Server();
		var port = server.Start();
		var client = new Client(port);
		await client.Start();

		client.Send("Message1\n");
		client.Send("Message2\n");
		client.Send("Message3\n");
		await client.DisposeAsync();

		var deadline = DateTimeOffset.Now.AddSeconds(30);
		while (server.Requests.Count < 3)
		{
			if (deadline < DateTimeOffset.Now)
				Assert.Fail("Timed out waiting for responses");
			await Task.Delay(50);
		}

		await server.DisposeAsync();

		Assert.Collection(
			server.Requests,
			msg => Assert.Equal("Message1", msg),
			msg => Assert.Equal("Message2", msg),
			msg => Assert.Equal("Message3", msg)
		);
	}

	class Client : IAsyncDisposable
	{
		readonly BufferedTcpClient bufferedClient;
		readonly int port;
		readonly Socket socket;

		public Client(int port)
		{
			this.port = port;

			socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
			bufferedClient = new BufferedTcpClient(socket, request => Requests.Add(Encoding.UTF8.GetString(request.ToArray())));
		}

		public List<string> Requests { get; } = new();

		public async ValueTask DisposeAsync()
		{
			await bufferedClient.DisposeAsync();

			socket.Shutdown(SocketShutdown.Receive);
			socket.Shutdown(SocketShutdown.Send);
			socket.Close();
			socket.Dispose();
		}

		public void Send(string text) =>
			bufferedClient?.Send(Encoding.UTF8.GetBytes(text));

		public async ValueTask Start()
		{
			await socket.ConnectAsync(IPAddress.Loopback, port);
			bufferedClient.Start();
		}
	}

	class Server : IAsyncDisposable
	{
		BufferedTcpClient? bufferedClient;
		readonly List<Action> cleanupTasks = new();

		public List<string> Requests { get; } = new();

		public async ValueTask DisposeAsync()
		{
			if (bufferedClient != null)
				await bufferedClient.DisposeAsync();

			foreach (var cleanupTask in cleanupTasks)
				cleanupTask();
		}

		public void Send(string text) =>
			bufferedClient?.Send(Encoding.UTF8.GetBytes(text));

		public int Start()
		{
			var listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
			cleanupTasks.Add(() =>
			{
				listenSocket.Close();
				listenSocket.Dispose();
			});

			listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
			listenSocket.Listen(1);

			Task.Run(async () =>
			{
				var socket = await listenSocket.AcceptAsync();

				cleanupTasks.Add(() =>
				{
					socket.Shutdown(SocketShutdown.Receive);
					socket.Shutdown(SocketShutdown.Send);
					socket.Close();
					socket.Dispose();
				});

				bufferedClient = new BufferedTcpClient(socket, request => Requests.Add(Encoding.UTF8.GetString(request.ToArray())));
				bufferedClient.Start();
			});

			return ((IPEndPoint)listenSocket.LocalEndPoint!).Port;
		}
	}
}
