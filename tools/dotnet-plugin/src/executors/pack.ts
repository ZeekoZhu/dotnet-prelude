import { ExecutorContext, PromiseExecutor, logger } from '@nx/devkit';
import { PackExecutorSchema } from './schema';
import { execSync } from 'child_process';
import * as path from 'node:path';
import * as fs from 'node:fs';

const runExecutor: PromiseExecutor<PackExecutorSchema> = async (
  options: PackExecutorSchema,
  context: ExecutorContext
) => {
  try {
    const projectRoot =
      context.projectsConfigurations.projects[context.projectName].root;
    const args: string[] = ['pack'];

    if (options.configuration) {
      args.push('--configuration', options.configuration);
    }

    if (options.noBuild) {
      args.push('--no-build');
    }

    if (options.noRestore) {
      args.push('--no-restore');
    }

    if (options.nologo) {
      args.push('--nologo');
    }

    if (options.includeSymbols) {
      args.push('--include-symbols');
    }

    if (options.includeSource) {
      args.push('--include-source');
    }

    let outputDir: string | undefined;
    if (options.output) {
      outputDir = options.output;
    } else {
      outputDir = path.join(context.root, 'dist', 'pack', context.projectName);
    }

    if (fs.existsSync(outputDir)) {
      fs.rmdirSync(outputDir, { recursive: true });
    }
    fs.mkdirSync(outputDir, { recursive: true });
    args.push('--output', outputDir);

    if (options.verbosity) {
      args.push('--verbosity', options.verbosity);
    }

    if (options.extraParameters) {
      args.push(options.extraParameters);
    }

    const command = `dotnet ${args.join(' ')}`;
    logger.info(`Executing command: ${command}`);

    execSync(command, {
      cwd: projectRoot,
      stdio: 'inherit',
    });

    return {
      success: true,
    };
  } catch (error) {
    logger.error('Failed to execute dotnet pack');
    logger.error(error);
    return {
      success: false,
    };
  }
};

export default runExecutor;
