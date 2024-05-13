# About This Project

This project contains the public site for [https://xunit.net/](https://xunit.net/). When adding new source analysis rules, use the [rule template](RULE_TEMPLATE.md) provided.

To open an issue for this project, please visit the [core xUnit.net project issue tracker](https://github.com/xunit/xunit/issues).

## Building and Contribution

This site is built with Jekyll, which is based on Ruby, as this is the default system used by GitHub Pages. We use a C#-based build system.

In order to successfully view the content locally, you will need the following pre-requisites:

* [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Ruby 2.7.4](https://www.ruby-lang.org/en/documentation/installation/)
* [Bundler 2.1.4](https://bundler.io/) (after installing Ruby, run `gem install bundler --version 2.1.4`)

The site content lives in [`/site`](https://github.com/xunit/xunit/tree/gh-pages/site) and the output will placed in `/_site`. To build the static content, run the command `./build` from the root of your clone, via your command prompt (bash and PowerShell are supported). This will do a one-time transformation of the templates in `/site` to the static files in `/_site`.

If you are working interactively, you can run `./build serve` which will start Jekyll in a mode where it incrementally builds the site as needed, and will rebuild pages as you change the files. You can point your browser to [http://localhost:4000/](http://localhost:4000/) while the server is running to view the rendered content. Once you're finished, you can pretty Ctrl+C in the command prompt and the server will shut down.

_Note: if you see a warning about `Auto-regeneration may not work on some Windows versions` and you're running in WSL 2, you may safely ignore this. Auto-regeneration works in WSL 2 without issue, and the warning is based on some very early WSL 1 bugs with file system watchers._

# About xUnit.net

[<img align="right" src="https://xunit.net/images/dotnet-fdn-logo.png" width="100" />](https://www.dotnetfoundation.org/)

xUnit.net is a free, open source, community-focused unit testing tool for the .NET Framework. Written by the original inventor of NUnit v2, xUnit.net is the latest technology for unit testing C#, F#, VB.NET and other .NET languages. xUnit.net works with ReSharper, CodeRush, TestDriven.NET and Xamarin. It is part of the [.NET Foundation](https://www.dotnetfoundation.org/), and operates under their [code of conduct](https://dotnetfoundation.org/about/policies/code-of-conduct). It is licensed under [Apache 2](https://opensource.org/licenses/Apache-2.0) (an OSI approved license).

For project documentation, please visit the [xUnit.net project home](https://xunit.net/).
