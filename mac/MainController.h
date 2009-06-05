#import <Cocoa/Cocoa.h>

@interface MainController : NSObject {
	NSMutableArray *processes;
	IBOutlet NSTextField *textOpenDnsAccount;
	IBOutlet NSTextField *textTicketNo;
	IBOutlet NSTextField *textDomainToTest;	
}

- (IBAction) runTests:(id)sender;
- (void) startTests;
- (void) processFinished:(NSNotification*)aNotification;
- (unsigned)finishedTasksCount;

@end
