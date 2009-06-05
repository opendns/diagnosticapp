#import <Cocoa/Cocoa.h>

@interface MainController : NSObject {
	IBOutlet NSTextField *openDnsAccount;
	IBOutlet NSTextField *ticketNo;
	IBOutlet NSTextField *domainToTest;	
}

-(IBAction) runTests:(id)sender;

@end
