/** Pack executor options for dotnet pack command */
export interface PackExecutorSchema {
  /**
   * Defines the build configuration. The default for most projects is Debug,
   * but you can override the build configuration settings in your project
   * @default "Debug"
   */
  configuration?: string;

  /** Extra command-line arguments that are passed verbatim to the dotnet command */
  extraParameters?: string;

  /** Include packages with symbols in addition to regular packages in output directory */
  includeSymbols?: boolean;

  /** Include PDBs and source files. Source files go into the 'src' folder in the resulting nuget package */
  includeSource?: boolean;

  /** Doesn't execute an implicit build during pack. Implies --no-restore */
  noBuild?: boolean;

  /** Doesn't execute an implicit restore during build */
  noRestore?: boolean;

  /** Doesn't display the startup banner or the copyright message. Available since .NET Core 3.0 SDK */
  nologo?: boolean;

  /**
   * Directory in which to place the built binaries.
   * If not specified, the default path is ./bin/<configuration>/.
   */
  output?: string;

  /**
   * Sets the verbosity level of the command
   * @default "minimal"
   */
  verbosity?: string;
}
