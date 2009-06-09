#import "Process.h"

@implementation Process

- (void)dealloc
{
	[displayName release];
	[super dealloc];
}

- (void)buildDisplayName:(NSString*)exePath withArgs:(NSArray*)args comment:(NSString*)aComment
{
	NSMutableString *s = [NSMutableString stringWithString:exePath];
	unsigned count = [args count];
	for (unsigned i = 0; i < count; i++)
	{
		NSString *tmp = [args objectAtIndex:i];
		[s appendFormat:@" %@", tmp];
	}
	if (aComment)
		[s appendFormat:@" %@", aComment];
	displayName = [s retain];
}

- (void)start:(NSString*)exePath withArgs:(NSArray*)args comment:(NSString*)aComment
{
	[self buildDisplayName:exePath withArgs:args comment:aComment];

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
    stdOut = [[NSString alloc] initWithData: data encoding: NSUTF8StringEncoding];

    file = [pipeStdErr fileHandleForReading];
	data = [file readDataToEndOfFile];
    stdErr = [[NSString alloc] initWithData: data encoding: NSUTF8StringEncoding];
	[task release];

	finished = TRUE;
}

- (BOOL)isFinished
{
	return finished;
}

- (NSString *)getResult
{
	NSMutableString *res = [NSMutableString stringWithCapacity:1024];
	[res appendString:@"---------------------------------------------\n"];
	[res appendString:@"Results for: "];
	[res appendString:displayName];
	[res appendString:@"\n"];

	if (stdOut && [stdOut length] > 0)
	{
		[res appendString:@"stdout:\n"];
		[res appendString:stdOut];
		[res appendString:@"\n"];
	}

	if (stdErr && [stdErr length] > 0)
	{
		[res appendString:@"stderr:\n"];
		[res appendString:stdErr];
		[res appendString:@"\n"];
	}
	return res;
}

@end
