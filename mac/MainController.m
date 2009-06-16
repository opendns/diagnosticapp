#import "MainController.h"
#import "Process.h"
#import "Http.h"

static NSString *REPORT_SUBMIT_URL = @"http://opendnsupdate.appspot.com/diagnosticsubmit";
//static NSString *REPORT_SUBMIT_URL = @"http://127.0.0.1:8080/diagnosticsubmit";

@interface MainController (Private)
- (void)onHttpDone;
- (void)onHttpError;
- (void)setTextView:(NSTextView*)textView string:(NSString*)aString;
- (void)hiliteAndActivateURLs:(NSTextView*)textView;
- (void)rememberAccountAndTicketNo;
- (BOOL)didAllTestsFinished;
- (void)startTests;
- (void)updateProgress;
@end

@implementation MainController

- (void)awakeFromNib
{
	processes = [[NSMutableArray alloc] initWithCapacity:20];

	[[NSApplication sharedApplication] setDelegate:self];
	//[self setTextView:textResultsLink string:@"See results at http://opendnsupdate.appspot.com/d/24011"];
	[textResultsLinkView setHidden:TRUE];

	NSString *title = [NSString stringWithFormat:@"OpenDNS Diagnostic v%@", 
					  [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleVersion"]];
	[window setTitle:title];
}

- (void)setTextView:(NSTextView*)textView string:(NSString*)aString
{
	NSTextStorage *ts = [textView textStorage];
	[ts beginEditing];
	NSString *s = [ts string];
	unsigned len = [s length];
	NSRange r = NSMakeRange(0, len);
	[ts deleteCharactersInRange:r];
	
	NSAttributedString *attrString =
	[[NSAttributedString alloc] initWithString:aString
									attributes:nil];
	[ts setAttributedString:attrString];
	[ts endEditing];
	[self hiliteAndActivateURLs:textView];
}

- (void)hiliteAndActivateURLs:(NSTextView*)textView
{
	NSTextStorage* textStorage=[textView textStorage];
	NSString* string=[textStorage string];
	NSRange searchRange=NSMakeRange(0, [string length]);
	NSRange foundRange;

	[textStorage beginEditing];
	do {
		foundRange=[string rangeOfString:@"http://" options:0 range:searchRange];
		
		if (foundRange.length > 0) {
			NSURL* theURL;
			NSDictionary* linkAttributes;
			NSRange endOfURLRange;
			
			searchRange.location=foundRange.location+foundRange.length;
			searchRange.length = [string length]-searchRange.location;
			
			endOfURLRange=[string rangeOfCharacterFromSet:
						   [NSCharacterSet whitespaceAndNewlineCharacterSet]
												  options:0 range:searchRange];
			
			if (endOfURLRange.length==0)
				endOfURLRange.location=[string length]-1;

			foundRange.length = endOfURLRange.location-foundRange.location+1;
			theURL=[NSURL URLWithString:[string substringWithRange:foundRange]];			
			linkAttributes= [NSDictionary dictionaryWithObjectsAndKeys: theURL, NSLinkAttributeName,
							 [NSNumber numberWithInt:NSSingleUnderlineStyle], NSUnderlineStyleAttributeName,
							 [NSColor blueColor], NSForegroundColorAttributeName,
							 NULL];

			[textStorage addAttributes:linkAttributes range:foundRange];
		}
	} while (foundRange.length!=0);
	
	[textStorage endEditing];
}

- (BOOL)applicationShouldTerminateAfterLastWindowClosed:(NSApplication *)theApplication
{
	return YES;
}

- (void)deallocate
{
	[results release];
}

- (void)disableUI
{
	[buttonStartTests setEnabled:FALSE];
	[textOpenDnsAccount setEnabled:FALSE];
	[textTicketNo setEnabled:FALSE];
	[textDomainToTest setEnabled:FALSE];
}

- (void)enableUI
{
	[buttonStartTests setEnabled:TRUE];
	[textOpenDnsAccount setEnabled:TRUE];
	[textTicketNo setEnabled:TRUE];
	[textDomainToTest setEnabled:TRUE];
}

- (void)showProgress
{
	[self updateProgress];
	[textResultsLinkView setHidden:TRUE];
	[textStatus setHidden:FALSE];
	[progressIndicator setHidden:FALSE];
	[progressIndicator startAnimation:nil];
}

- (void)hideProgress
{
	[textStatus setHidden:TRUE];
	[progressIndicator stopAnimation:nil];
	[progressIndicator setHidden:TRUE];	
}

- (void)updateProgress
{
	unsigned count = [processes count];
	unsigned finished = [self finishedTasksCount];
	NSString *s = [NSString stringWithFormat:@"Please wait. Finished %d out of %d tests.", (int)finished, (int)count];
	[textStatus setStringValue:s];
}

- (IBAction)runTests:(id)sender
{
	[self startTests];
}

- (void)startTest:(NSString*)exe withArgs:(NSArray*)args comment:(NSString*)aComment
{
	Process * process = [[Process alloc] init];
	[[NSNotificationCenter defaultCenter] addObserver:self
											 selector:@selector(processTerminatedNotification:) 
												 name:ProcessTerminatedNotification 
											   object:process];
	
	[processes addObject:process];
	[process start:exe withArgs:args comment:aComment];
}

- (void)startTest:(NSString*)exe withArgs:(NSArray*)args
{
	[self startTest:exe withArgs:args comment:nil];
}

- (void)startPing:(NSString*)addr comment:(NSString*)aComment
{
	NSArray *args = [NSArray arrayWithObjects: @"-c", @"5", addr, nil];
	[self startTest:@"/sbin/ping" withArgs:args comment:aComment];
}

- (void)startTraceroute:(NSString*)addr
{
	NSArray *args = [NSArray arrayWithObjects: @"-I", @"-w 2", addr, nil];
	[self startTest:@"/usr/sbin/traceroute" withArgs:args comment:nil];
}

- (void)freeAccountAndTicketNo
{
	[txtOpenDnsAccount release];
	txtOpenDnsAccount = nil;
	[txtTicketNo release];
	txtTicketNo = nil;
}

- (void)rememberAccountAndTicketNo
{
	[self freeAccountAndTicketNo];
	NSString *s;
	s = [textOpenDnsAccount stringValue];
	s = [s stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
	if (s && [s length] > 0) {
		txtOpenDnsAccount = [s copy];
	}

	s = [textTicketNo stringValue];
	s = [s stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
	if (s && [s length] > 0) {
		txtTicketNo = [s copy];
	}
}

- (void)startTests
{
	NSArray *args;

	[self rememberAccountAndTicketNo];
	[self disableUI];
	[processes removeAllObjects];

	NSString *host = [textDomainToTest stringValue];
	NSRange range = [host rangeOfString:@"."];
	if (range.location != NSNotFound)
		[self startTraceroute:host];

	[self startTraceroute:@"208.67.222.222"];
	[self startTraceroute:@"208.67.220.220"];

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

	args = [NSArray arrayWithObject: @"aux"];
	[self startTest:@"/bin/ps" withArgs:args];

	args = [NSArray arrayWithObject: @"-a"];
	[self startTest:@"/sbin/ifconfig" withArgs:args];
	
	[self showProgress];
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

- (void)buildResults
{
	NSMutableString *res = [NSMutableString stringWithCapacity:2048];
	if (txtOpenDnsAccount) {
		[res appendString:@"OpenDNS account: "];
		[res appendString:txtOpenDnsAccount];
		[res appendString:@"\n"];
	}

	if (txtTicketNo) {
		[res appendString:@"OpenDNS ticket: "];
		[res appendString:txtTicketNo];
		[res appendString:@"\n"];
	}

	unsigned count = [processes count];
	for (unsigned i = 0; i < count; i++) {
		Process *process = [processes objectAtIndex:i];
		NSString *s = [process getResult];
		[res appendString:s];
	}
	results = [res retain];		
}

- (void)onHttpDone:(Http*)aHttp
{
	resultsUrl = [aHttp reply];
	[resultsUrl retain];
	[aHttp release];

	NSString *s = [NSString stringWithFormat:@"See results at %@", resultsUrl];
	[self setTextView:textResultsLink string:s];
	[textResultsLinkView setHidden:FALSE];
}

- (void)onHttpError:(Http*)aHttp
{
	[aHttp release];	
	[self setTextView:textResultsLink string:@"Error submitting the results"];
	[textResultsLinkView setHidden:FALSE];
}

- (void)submitResults
{
	const char *utf8 = [results UTF8String];
	unsigned len = strlen(utf8);
	NSData *data = [NSData dataWithBytes:(const void*)utf8 length:len];
	NSURL *url = [NSURL URLWithString:REPORT_SUBMIT_URL];
	[[Http alloc] initWithURL:url
						 data:data
					 delegate:self
				 doneSelector:@selector(onHttpDone:)
				errorSelector:@selector(onHttpError:)];
}

- (void)handleAllTestsFinished
{
	[self buildResults];
	[self submitResults];
	[self enableUI];
	[self hideProgress];
}

- (void)processTerminatedNotification:(NSNotification *)aNotification
{
	[self updateProgress];
	if ([self didAllTestsFinished])
		[self handleAllTestsFinished];		
}

@end
