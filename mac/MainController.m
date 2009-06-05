#import "MainController.h"
#import "Process.h"

@implementation MainController

- (void)awakeFromNib
{
	processes = [[NSMutableArray alloc] initWithCapacity:20];

    [[NSNotificationCenter defaultCenter] addObserver:self
											 selector:@selector(processFinished:)
												 name:NSTaskDidTerminateNotification
											   object:nil];
}

- (IBAction) runTests:(id)sender
{
	[self startTests];
}

- (void) startTest:(NSString*)exe withArgs:(NSArray*)args
{
	Process * process = [[Process alloc] init];
	[processes addObject:process];
	[process start:exe withArgs:args];
}

- (void) startTests
{
	[processes removeAllObjects];
	NSArray *args = [NSArray arrayWithObjects: @"-type=txt", @"which.opendns.com.", @"208.67.222.222", nil];
	[self startTest:@"/usr/bin/nslookup" withArgs:args];
}

- (Process*)findProcessByTask:(NSTask*)aTask
{
	unsigned count = [processes count];
	for (unsigned i = 0; i < count; i++) {
		Process *process = [processes objectAtIndex:i];
		if ([process isProcessForTask:aTask])
			return process;
	}
	return nil;	
}

- (unsigned)finishedTasksCount
{
	unsigned count = [processes count];
	unsigned finishedCount = 0;
	for (unsigned i = 0; i < count; i++) {
		Process *process = [processes objectAtIndex:i];
		if ([process isFinished])
			finishedCount += 1;
	}
	return finishedCount;
}

- (BOOL)allTasksFinished
{
	unsigned count = [processes count];
	for (unsigned i = 0; i < count; i++) {
		Process *process = [processes objectAtIndex:i];
		if (![process isFinished])
			return FALSE;
	}
	return TRUE;
}

- (void)processFinished:(NSNotification *)aNotification
{
	NSTask *task = [aNotification object];
	Process *process = [self findProcessByTask:task];
	[process finish];
}

@end
