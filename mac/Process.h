#import <Cocoa/Cocoa.h>

@interface Process : NSObject {
	NSTask *task;
	NSPipe *pipeStdOut;
	NSPipe *pipeStdErr;
	
	NSString *stdOutUtf8;
	NSString *stdErrUtf8;

	NSString *comment;

	BOOL finished;
}

- (void)start:(NSString*)exe withArgs:(NSArray*)args comment:(NSString*)aComment;
- (void)finish;
- (BOOL)isProcessForTask:(NSTask*)aTask;
- (BOOL)isFinished;

@end
