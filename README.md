# 0xced.Templates

0xced's opinionated templates for `dotnet new`

## Installation

To install the templates (or update to the latest version), run the following command:

```sh
git clone https://github.com/0xced/0xced.Templates
dotnet new install --force 0xced.Templates
```

For more information about the `dotnet new` command, see the the official [dotnet new documentation][dotnet-new].

## The `nuget-classlib` template

The NuGet class library template creates a class library and everything needed for publishing on NuGet, including a GitHub Actions workflow. Compared to the built-in `classlib` template, the `nuget-classlib` provides many additional features.

To create a solution from this template, run the following commands:

```sh
mkdir MyGreatLibrary && cd MyGreatLibrary
dotnet new nuget-classlib
```

### Class library project

* 🎯 Target .NET 8 which has [Long Term Support][LTS], i.e. is supported for three years after the initial release.
* ❔ Enable [nullable reference types][NRT] with `<Nullable>enable</Nullable>` (same as the built-in `classlib` template).
* ⚠️ [Treat warnings as errors][TWAS], because ignoring warnings leads to bugs, and errors can't be ignored.
* 🔬 Enable all [.NET source code analysis][CodeAnalysis] rules with `<AnalysisMode>All</AnalysisMode>`.
  This can turn out rather extreme (especially with warning as errors) so be prepared to [suppress some warnings][SuppressWarnings] or maybe even [downgrade][AnalysisMode] from `All` to `Recommended` or `Minimum`.
* 📗 Generate a [documentation file][XmlDoc]. Together with warnings as errors, forgetting a comment on a public member becomes a compilaiton error.
* 🔗 Integrate [Source Link][SourceLink] to make debugging the library a bliss. Also, with `<DebugType>embedded</DebugType>` so that having the NuGet package is enough to ensure a great debugging experience, even if your internet connection goes down.
* 📦 Enable [package validation][PackageValidation] to ensure that a [breaking change][BreakingChanges] doesn't inadvertently slip in a minor or patch version.
* ℹ️ Define all important metadata to follow the [package authoring best practices][PackageAuthoringBestPractices].
  * Authors
  * Copyright
  * Description
  * PackageIcon
  * [PackageReadmeFile][PackageReadmeFile]
  * PackageLicenseExpression
  * PackageTags
  * PackageProjectUrl
  * PackageReleaseNotes
  * PublishRepositoryUrl
* 🏷 Versioning with [MinVer][MinVer], the best .NET versioning tool.
* 🔒 Use a [lock file][LockFile] for repeatable restore.
* ✅ Automatically validate that the package was produced by a [deterministic build][DeterministicBuild] and that Source Link is property configured.

### Test project

* 👩‍🔬 Code coverage with [coverlet][coverlet].
  * Running `dotnet test` automatically runs coverage, displays a summary on your terminal, generates an html report with [ReportGenerator][ReportGenerator] and opens it in your browser.
  * Deterministic report paths, working around [microsoft/vstest/#2378][VsTestIssue2378] (Avoid guid at the end of outputDirectory).
* ⚔️ Using [xUnit.net][xUnit], the best unit testing tool of the .NET ecosystem. Still on xUnit.net v2 because [v3 is incompatible with Strkyer](https://github.com/stryker-mutator/stryker-net/issues/3117) and [TUnit](https://tunit.dev) has critical issues with JetBrains Rider, my IDE of choice:
  * [RIDER-127224](https://youtrack.jetbrains.com/issue/RIDER-127224/Jump-to-source-with-TUnit-doesnt-work) Jump to source with TUnit doesn't work
  * [DCVR-12871](https://youtrack.jetbrains.com/issue/DCVR-12871/Not-receiving-coverage-from-TUnit-Testing-Platform-tests-in-dotCover) Not receiving coverage from TUnit (Testing Platform) tests in dotCover
* ⚙️ Public API snapshot with [Verify][Verify] for easy review of your public API evolution.

### GitHub Actions workflow

The ***Continuous Integration*** workflow runs everytime a commit is pushed to the GitHub repository.

A package (nupkg file) is always produced and available in the action artifacts. When the commit has an attached tag, which is required when using [MinVer][MinVer], the package is published on NuGet.

### 🛠 The `package` job

The package job runs on Linux + macOS + Windows. This ensures the code is truly cross platform. `fail-fast` is set to`false` so that if an error occurs one one platform, other platforms continue to run the job.

This job builds the NuGet package and runs the unit tests.

The following steps are performed.

* 🧑‍💻 Chekout the git repository, with `fetch-depth: 0` so that MinVer can version pre-release versions correctly.
* 🏎 Optimize the Windows runner because it's slow. See the [extremely slow Network/Disk IO on Windows agent compared to Ubuntu/Mac issue](https://github.com/actions/setup-dotnet/issues/260) on GitHub.
* 🧑‍🔧 Install the .NET SDK. No parameters are specified so that the SDK version from the `global.json` file is used. And since `latestFeature` is used as the roll forward policy, the [dotnet-install-script](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script) is executed with `--channel` and the two-part version in A.B format.
* ℹ️ Display the result of `dotnet --info` because it's useful to have these technical details if anything goes wrong.
* 💾 Taking advantage of the `packages.lock.json` file, NuGet package dependencies are retrived from a [cache](https://docs.github.com/en/actions/using-workflows/caching-dependencies-to-speed-up-workflows). If the packages are not in the cache anymore, they will be retrieved from nuget.org during the restore step.
* ⚙️ `dotnet restore` to restore the NuGet package dependencies.
* 🏗 `dotnet build --no-restore` to build the solution (class library + tests).
* 🧪 `dotnet test --no-build`  to run the tests.
* 📤 If the tests fail, all `**/*.received.*` files (produced by [Verify][Verify]) are uploaded as artifacts so that the can be analyzed. The trx file is always uploaded as an artifact, too.
* 📊 The [Test Reporter][TestReporter] action is run to make test results summary available directly on the action run. Another option could be to use the [dotnet-trx][dotnet-trx] tool which serves a similar purpose.
* ☂️ Code coverage results are uploaded to [Codecov][Codecov]. This requires to login to Codecov and setup the repositry first.
* ☂️ If the `CODACY_PROJECT_TOKEN` secret is defined in the repositry, code coverage results are also uplaoded to [Codacy][Codacy].
* 📦 `dotnet pack --no-build` is run to produce the NuGet package.
* 📤 The produced NuGet package is uploaded as an artifact.
* 👽 If the `STRYKER_DASHBOARD_API_KEY` secret is defined in the repositry, mutation testing is performed with [Stryker][Stryker] and the mutation results are uploaded to the Stryker dashboard.
* 📝 If a tag exists on the commit, release notes are extracted from the tag and a *Release Notes* artifact is uploaded.

### 🐿 The `publish` job

When a tag is attached to the commit, the following steps are performed.

* 🚢 The release notes artifact from the `package` job is downloaded and a GitHub release is created.
* 🔑 A NuGet API key is retrieved using [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing). This requires *Trusted Publishing* to be configured in your NuGet account.
* 🚀 Finally, the nupkg artifact from the `package` job is published on NuGet.

### Miscellaneous

*	Dependabot
*	README
*	CHANGELOG
*	Code of Conduct

## The `spectre-console` template

The Spectre Console template creates a console app powered by the [Spectre.Console.Cli](https://spectreconsole.net/cli/) package.

To create a project from this template, run the following commands:

```sh
mkdir mycli && cd mycli
dotnet new spectre-console
```

This template provides the following features.

* 🔢 Automatic  `--version` support using the [AssemblyInformationalVersion](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assemblyinformationalversionattribute) attribute of the main assembly. The version can be controlled by setting the `<Version>1.2.3</Version>` property in the csproj.
* 🎛️ A default command with just enough to get started with [settings](https://spectreconsole.net/cli/settings).
* 📄 Redirection-friendy stdout and stderr. I.e. get nice interactive [spinners](https://spectreconsole.net/live/status) and [progress bars](https://spectreconsole.net/live/progress) while running in a terminal and ignore those when redirected to a file.
* ⚡️ Cancellable commands with a cancellation token that triggers when Ctrl+C is pressed.
* 💣 A solid exception handler.
  * Exits with code 130 when Ctrl+C is pressed, code 64 (EX_USAGE) if the app is used incorrectly, code 70 (EX_SOFTWARE) in case of uncaught exception.
  * Pretty display of exceptions.
  * Writes the help page in case the app is misued.

[dotnet-new]: https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new
[LTS]: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
[NRT]: https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references
[TWAS]: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings#treatwarningsaserrors
[CodeAnalysis]: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview
[SuppressWarnings]: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/suppress-warnings
[AnalysisMode]: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview#enable-additional-rules
[XmlDoc]: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/
[SourceLink]: https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink
[PackageValidation]: https://learn.microsoft.com/en-us/dotnet/fundamentals/package-validation/overview
[BreakingChanges]: https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/breaking-changes
[PackageAuthoringBestPractices]: https://learn.microsoft.com/en-us/nuget/create-packages/package-authoring-best-practices
[PackageReadmeFile]: https://devblogs.microsoft.com/nuget/add-a-readme-to-your-nuget-package/
[MinVer]: https://github.com/adamralph/minver
[LockFile]: https://devblogs.microsoft.com/nuget/enable-repeatable-package-restores-using-a-lock-file/
[DeterministicBuild]: https://devblogs.microsoft.com/dotnet/producing-packages-with-source-link/#deterministic-builds
[coverlet]: https://github.com/coverlet-coverage/coverlet
[ReportGenerator]: https://reportgenerator.io
[VsTestIssue2378]: https://github.com/microsoft/vstest/issues/2378
[xUnit]: https://xunit.net
[Verify]: https://github.com/VerifyTests/Verify
[TestReporter]: https://github.com/dorny/test-reporter
[dotnet-trx]: https://github.com/devlooped/dotnet-trx
[Codecov]: https://codecov.io
[Codacy]: https://www.codacy.com
[Stryker]: https://stryker-mutator.io
[SpectreConsole]: https://spectreconsole.net
