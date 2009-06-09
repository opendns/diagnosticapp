#import <Cocoa/Cocoa.h>

@interface Process : NSObject {
	NSTask *task;
	NSPipe *pipeStdOut;
	NSPipe *pipeStdErr;
	
	NSString *stdOut;
	NSString *stdErr;

	NSString *displayName;

	BOOL finished;
}

- (void)start:(NSString*)exe withArgs:(NSArray*)args comment:(NSString*)aComment;
- (void)finish;
- (BOOL)isProcessForTask:(NSTask*)aTask;
- (BOOL)isFinished;
- (NSString *)getResult;

@end
