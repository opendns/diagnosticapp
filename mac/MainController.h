#import <Cocoa/Cocoa.h>

#define APP_VER @"0.1"

@interface MainController : NSObject {
	NSMutableArray *processes;
	
	IBOutlet NSButton *buttonStartTests;
	IBOutlet NSTextField *textOpenDnsAccount;
	IBOutlet NSTextField *textTicketNo;
	IBOutlet NSTextField *textDomainToTest;	
	
	IBOutlet NSProgressIndicator *progressIndicator;
	IBOutlet NSTextField *textStatus;
}

- (IBAction) runTests:(id)sender;
- (void) startTests;
- (void) processFinished:(NSNotification*)aNotification;
- (unsigned)finishedTasksCount;

@end
