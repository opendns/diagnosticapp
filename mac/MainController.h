#import <Cocoa/Cocoa.h>

#define APP_VER @"0.2"

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
	IBOutlet NSWindow *		window;

	NSString *				txtOpenDnsAccount;
	NSString *				txtTicketNo;

	NSString *				results;
	NSString *				resultsUrl;
}

- (IBAction)runTests:(id)sender;
- (unsigned)finishedTasksCount;

@end
