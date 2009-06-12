#import "Process.h"

NSString * const ProcessTerminatedNotification = @"ProcessTerminatedNotification";

@interface Process (Private)
- (void)terminatedNotification:(NSNotification*)aNotification;
- (void)stdOutNotification:(NSNotification*)aNotification;
- (void)stdErrNotification:(NSNotification*)aNotification;
- (void)buildDisplayName:(NSString*)exePath withArgs:(NSArray*)args comment:(NSString*)aComment;
@end

@implementation Process

- (void)dealloc
{
	[displayName release];
	[stdOutData release];
	[stdErrData release];
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

    [task setStandardOutput: [NSPipe pipe]];	
    [task setStandardError: [NSPipe pipe]];	

	stdOutData = [[NSMutableData alloc] init];
	stdErrData = [[NSMutableData alloc] init];

    [[NSNotificationCenter defaultCenter] addObserver:self
											 selector:@selector(terminatedNotification:)
												 name:NSTaskDidTerminateNotification
											   object:task];

	NSFileHandle *stdOutHandle = [[task standardOutput] fileHandleForReading];
	[[NSNotificationCenter defaultCenter] addObserver:self
											 selector:@selector(stdOutNotification:) 
												 name:NSFileHandleDataAvailableNotification
											   object:stdOutHandle];
	[stdOutHandle waitForDataInBackgroundAndNotify];

	NSFileHandle *stdErrHandle = [[task standardError] fileHandleForReading];
	[[NSNotificationCenter defaultCenter] addObserver:self
											 selector:@selector(stdErrNotification:) 
												 name:NSFileHandleDataAvailableNotification
											   object:stdErrHandle];
	[stdErrHandle waitForDataInBackgroundAndNotify];

    [task launch];
}

- (void)terminatedNotification:(NSNotification*)aNotification
{
	NSFileHandle *	fileHandle;
	NSData *		data;

	NSTask *aTask = [aNotification object];
	NSAssert(task == aTask, @"");

	// mark as finished early, to disable further notifications on stdout and stderr
	finished = TRUE;

	fileHandle = [[task standardOutput] fileHandleForReading];
	data = [fileHandle readDataToEndOfFile];
	[stdOutData appendData:data];
	
	fileHandle = [[task standardError] fileHandleForReading];
	data = [fileHandle readDataToEndOfFile];
	[stdErrData appendData:data];
	
    stdOutStr = [[NSString alloc] initWithData: stdOutData 
									  encoding: NSUTF8StringEncoding];
    stdErrStr = [[NSString alloc] initWithData: stdErrData 
									  encoding: NSUTF8StringEncoding];
	[task release];
	[[NSNotificationCenter defaultCenter] postNotificationName:ProcessTerminatedNotification
														object:self];
}

- (void)stdOutNotification:(NSNotification*)aNotification
{
	// not sure why I'm getting those after the process has finished
	if (finished)
		return;
    NSFileHandle *stdOutFile = (NSFileHandle *)[aNotification object];
    [stdOutData appendData:[stdOutFile availableData]];
    [stdOutFile waitForDataInBackgroundAndNotify];	
}

- (void)stdErrNotification:(NSNotification*)aNotification
{
	// not sure why I'm getting those after the process has finished
	if (finished)
		return;
    NSFileHandle *stdErrFile = (NSFileHandle *)[aNotification object];
    [stdErrData appendData:[stdErrFile availableData]];
    [stdErrFile waitForDataInBackgroundAndNotify];	
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

	if (stdOutStr && [stdOutStr length] > 0)
	{
		[res appendString:@"stdout:\n"];
		[res appendString:stdOutStr];
		[res appendString:@"\n"];
	}

	if (stdErrStr && [stdErrStr length] > 0)
	{
		[res appendString:@"stderr:\n"];
		[res appendString:stdErrStr];
		[res appendString:@"\n"];
	}
	return res;
}

@end
