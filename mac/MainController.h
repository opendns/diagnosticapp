#import <Cocoa/Cocoa.h>

#define APP_VER @"0.1"

@interface MainController : NSObject {
	NSMutableArray *		processes;
	
	IBOutlet NSButton *		buttonStartTests;
	IBOutlet NSTextField *	textOpenDnsAccount;
	IBOutlet NSTextField *	textTicketNo;
	IBOutlet NSTextField *	textDomainToTest;	
	
	IBOutlet NSProgressIndicator *progressIndicator;
	IBOutlet NSTextField *	textStatus;
	IBOutlet NSScrollView * textResultsLinkView;
	IBOutlet NSTextView *	textResultsLink;
	
	NSString *				results;
	NSString *				resultsUrl;
}

- (IBAction) runTests:(id)sender;
- (IBAction) gotoResultsUrl:(id)sender;
- (void) startTests;
- (void) processFinished:(NSNotification*)aNotification;
- (unsigned)finishedTasksCount;

@end
