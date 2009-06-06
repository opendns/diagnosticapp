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

- (void) startTest:(NSString*)exe withArgs:(NSArray*)args comment:(NSString*)aComment
{
	Process * process = [[Process alloc] init];
	[processes addObject:process];
	[process start:exe withArgs:args comment:aComment];
}

- (void) startTest:(NSString*)exe withArgs:(NSArray*)args
{
	[self startTest:exe withArgs:args comment:nil];
}

- (void) startPing:(NSString*)addr comment:(NSString*)aComment
{
	NSArray *args = [NSArray arrayWithObjects: @"-n", @"5", addr, nil];
	[self startTest:@"/sbin/ping" withArgs:args comment:aComment];
}

- (void) startTraceroute:(NSString*)addr
{
	NSArray *args = [NSArray arrayWithObject: addr];
	[self startTest:@"/usr/sbin/traceroute" withArgs:args comment:nil];
}

- (void) startTests
{
	NSArray *args;

	[self disableUI];
	[processes removeAllObjects];

	//Tests.Add(new DnsResolveStatus("myip.opendns.com"));

	NSString *host = [textDomainToTest stringValue];
	NSRange range = [host rangeOfString:@"."];
	if (range.location != NSNotFound)
		[self startTraceroute:host];
		
	//[self startTraceroute:@"208.67.222.222"];
	//[self startTraceroute:@"208.67.220.220"];

	args = [NSArray arrayWithObject:@"myip.opendns.com"];
	[self startTest:@"/usr/bin/nslookup" withArgs:args];

	args = [NSArray arrayWithObjects: @"-type=txt", @"which.opendns.com.", @"208.67.222.222", nil];
	[self startTest:@"/usr/bin/nslookup" withArgs:args];

	args = [NSArray arrayWithObjects: @"-type=txt", @"-port=5353", @"which.opendns.com.", @"208.67.222.222", nil];
	[self startTest:@"/usr/bin/nslookup" withArgs:args];
	
	args = [NSArray arrayWithObjects: @"-class=chaos", @"-type=txt", @"hostname.bind.", @"4.2.2.1", nil];
	[self startTest:@"/usr/bin/nslookup" withArgs:args];

	args = [NSArray arrayWithObjects:  @"-class=chaos", @"-type=txt", @"hostname.bind.", @"192.33.4.12", nil];
	[self startTest:@"/usr/bin/nslookup" withArgs:args];

	args = [NSArray arrayWithObjects: @"-class=chaos", @"-type=txt", @"hostname.bind.", @"204.61.216.4", nil];
	[self startTest:@"/usr/bin/nslookup" withArgs:args];

	args = [NSArray arrayWithObjects: @"whoami.ultradns.net", @"udns1.ultradns.net", nil];
	[self startTest:@"/usr/bin/nslookup" withArgs:args];

	args = [NSArray arrayWithObjects: @"-debug", @"debug.opendns.com.", nil];
	[self startTest:@"/usr/bin/nslookup" withArgs:args];

	[self startPing:@"208.67.219.99" comment:@"(www.opendns.com)"];
	[self startPing:@"208.67.219.1" comment:@"(palo alto router)"];
	[self startPing:@"208.67.216.1" comment:@"(seattle router)"];
	[self startPing:@"208.69.36.1" comment:@"(chicago router)"];
	[self startPing:@"208.67.217.1" comment:@"(new york router)"];
	[self startPing:@"208.69.32.1" comment:@"(ashburn router)"];
	[self startPing:@"208.69.34.1" comment:@"(london router)"];
	[self startPing:@"209.244.5.114" comment:@"(level3 west coast)"];
	[self startPing:@"209.244.7.33" comment:@"(level3 east coast)"];
	[self startPing:@"192.153.156.3" comment:@"(att west coast)"];
	[self startPing:@"207.252.96.3" comment:@"(att east coast)"];

	args = [NSArray arrayWithObject: @"ax"];
	[self startTest:@"/bin/ps" withArgs:args];

	args = [NSArray arrayWithObject: @"-a"];
	[self startTest:@"/sbin/ifconfig" withArgs:args];
	
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
