#import <Cocoa/Cocoa.h>

@interface Http : NSObject {
    NSURL *         serverURL;
    id              delegate;
    NSString *      reply;
    SEL             doneSelector;
    SEL             errorSelector;
    BOOL            uploadDidSucceed;
}

- (id)initWithURL: (NSURL *)serverURL
             data: (NSData *)data
         delegate: (id)delegate
     doneSelector: (SEL)doneSelector
    errorSelector: (SEL)errorSelector;

- (NSString*)reply;

@end
