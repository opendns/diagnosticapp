#import "Process.h"

@implementation Process

- (void)dealloc
{
	[comment release];
	[super dealloc];
}

- (void)start:(NSString*)exePath withArgs:(NSArray*)args comment:(NSString*)aComment
{
	comment = [aComment retain];
	task = [[NSTask alloc] init];
    [task setLaunchPath: exePath];
    [task setArguments: args];	

    pipeStdOut = [NSPipe pipe];
    [task setStandardOutput: pipeStdOut];	

    pipeStdErr = [NSPipe pipe];
    [task setStandardError: pipeStdErr];	

    [task launch];
}

- (BOOL)isProcessForTask:(NSTask*)aTask
{
	if (task == aTask)
		return TRUE;
	return FALSE;
}

- (void)finish
{
    NSFileHandle *file = [pipeStdOut fileHandleForReading];
    NSData *data = [file readDataToEndOfFile];
    stdOutUtf8 = [[NSString alloc] initWithData: data encoding: NSUTF8StringEncoding];

    file = [pipeStdErr fileHandleForReading];
	data = [file readDataToEndOfFile];
    stdErrUtf8 = [[NSString alloc] initWithData: data encoding: NSUTF8StringEncoding];
	[task release];

	finished = TRUE;
}

- (BOOL)isFinished
{
	return finished;
}

@end
