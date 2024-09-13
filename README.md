# About This Project

This project contains the public site for [https://xunit.net/](https://xunit.net/).

To open an issue for this project, please visit the [core xUnit.net project issue tracker](https://github.com/xunit/xunit/issues).

## Building and Contribution

This site is built with Jekyll, which is based on Ruby, as this is the default system used by GitHub Pages. We use a C#-based build system.

### Build prerequisites

In order to successfully view the content locally, you will need the following pre-requisites:

* [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Ruby 3.3.0](https://www.ruby-lang.org/en/documentation/installation/)
* [Bundler 2.5.3](https://bundler.io/) (the default with Ruby 3.3.0; you can run `gem install bundler --version 2.5.3` if needed)

We have verified this works using both Windows and Linux, though using Windows might try to add additional Windows-only dependencies to [`Gemfile.lock`](Gemfile.lock). We recommend Windows users a Linux distribution via WSL 2 and [VSCode](https://code.visualstudio.com/) for the smoothest experience.

### Editing the site pages

The site content lives in [`/site`](https://github.com/xunit/xunit/tree/gh-pages/site) and the output will placed in `/_site`. To build the static content, run the command `./build` from the root of your clone, via your command prompt (bash and PowerShell are supported). This will do a one-time transformation of the templates in `/site` to the static files in `/_site`.

For working interactively, you can run `./build serve` which will start Jekyll in a mode where it incrementally builds the site as needed, and will rebuild pages as you change the files. You can point your browser to [http://localhost:4000/](http://localhost:4000/) while the server is running to view the rendered content. Once you're finished, you can press Ctrl+C in the command prompt and the server will shut down.

_Note: if you see a warning about `Auto-regeneration may not work on some Windows versions` and you're running in WSL 2, you may safely ignore this. Auto-regeneration works in WSL 2 without issue, and the warning is based on some very early WSL 1 bugs with file system watchers._

Text editors/IDEs which understand site hierarchy and linking while editing Markdown and HTML are strongly encouraged to open the `/site` folder and not the root folder when editing content, so that the editor understands where the content root lives. _For example, if you're using VSCode, you should run `code site` and not `code .` from the root of the repo._ You should only ever need to open the root of the repo in your editor if you're working on the build tools.

### Editing analyzer rule documentation

The analyzer rules are does as Markdown templates that live in [`/site/xunit.analyzers/_rules`](site/xunit.analyzers/_rules) and are rendered dynamically into [`/site/xunit.analyzers/_rules`](site/xunit.analyzers/rules).

The standard template for creating documentation for a new rule lives in [`/site/xunit.analyzers/_rules/_stub.md`](site/xunit.analyzers/_rules/_stub.md). Copy this file and name it to match the analyzer ID (note: these are case-sensitive, so they should always have names like `xUnit0000.md` where `0000` is the rule number). Fill out the document header and contents, and ensure the rule shows up in the [Analyzer Rules](http://localhost:4000/xunit.analyzers/rules) home page. Ensure that the page renders properly, as you may need to quote values in the header if they contain characters that Jekyll does not to process when running the template engine. You can use an existing documentation page to understand what an analyzer documentation page should look like.

# About xUnit.net

[<img align="right" src="https://xunit.net/images/dotnet-fdn-logo.png" width="100" />](https://www.dotnetfoundation.org/)

xUnit.net is a free, open source, community-focused unit testing tool for the .NET Framework. Written by the original inventor of NUnit v2, xUnit.net is the latest technology for unit testing C#, F#, VB.NET and other .NET languages. xUnit.net works with ReSharper, CodeRush, TestDriven.NET and Xamarin. It is part of the [.NET Foundation](https://www.dotnetfoundation.org/), and operates under their [code of conduct](https://dotnetfoundation.org/about/policies/code-of-conduct). It is licensed under [Apache 2](https://opensource.org/licenses/Apache-2.0) (an OSI approved license).

For project documentation, please visit the [xUnit.net project home](https://xunit.net/).
