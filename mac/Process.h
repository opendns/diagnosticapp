#import <Cocoa/Cocoa.h>

extern NSString * const ProcessTerminatedNotification;

@interface Process : NSObject {
	NSTask *		task;
	
	NSMutableData *	stdOutData;
	NSMutableData *	stdErrData;
	NSString *		stdOutStr;
	NSString *		stdErrStr;

	NSString *		displayName;

	BOOL			finished;
}

- (void)start:(NSString*)exe withArgs:(NSArray*)args comment:(NSString*)aComment;
- (BOOL)isFinished;
- (NSString *)getResult;

@end
