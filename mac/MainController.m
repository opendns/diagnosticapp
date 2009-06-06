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

- (void) disableUI
{
	[buttonStartTests setEnabled:FALSE];
	[textOpenDnsAccount setEnabled:FALSE];
	[textTicketNo setEnabled:FALSE];
	[textDomainToTest setEnabled:FALSE];
}

- (void) enableUI
{
	[buttonStartTests setEnabled:TRUE];
	[textOpenDnsAccount setEnabled:TRUE];
	[textTicketNo setEnabled:TRUE];
	[textDomainToTest setEnabled:TRUE];
}

- (void) showProgress
{
	[textStatus setHidden:FALSE];
	[progressIndicator setHidden:FALSE];
}

- (void) hideProgress
{
	[textStatus setHidden:TRUE];
	[progressIndicator setHidden:TRUE];	
}

- (void) updateProgress
{
	unsigned count = [processes count];
	unsigned finished = [self finishedTasksCount];
	NSString *s = [NSString stringWithFormat:@"Please wait. Finished %d out of %d tests.", (int)finished, (int)count];
	[textStatus setStringValue:s];
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
	[self disableUI];
	[processes removeAllObjects];
	NSArray *args = [NSArray arrayWithObjects: @"-type=txt", @"which.opendns.com.", @"208.67.222.222", nil];
	[self startTest:@"/usr/bin/nslookup" withArgs:args];
	[self showProgress];
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

- (BOOL)didAllTestsFinished
{
	unsigned count = [processes count];
	for (unsigned i = 0; i < count; i++) {
		Process *process = [processes objectAtIndex:i];
		if (![process isFinished])
			return FALSE;
	}
	return TRUE;
}

- (void)handleAllTestsFinished
{
	[self enableUI];
	[self hideProgress];
}

- (void)processFinished:(NSNotification *)aNotification
{
	NSTask *task = [aNotification object];
	Process *process = [self findProcessByTask:task];
	[process finish];
	[self updateProgress];
	if ([self didAllTestsFinished])
		[self handleAllTestsFinished];		
}

@end
