#import <Cocoa/Cocoa.h>

@interface Process : NSObject {
	NSTask *task;
	NSPipe *pipeStdOut;
	NSPipe *pipeStdErr;
	
	NSString *stdOutUtf8;
	NSString *stdErrUtf8;
	
	BOOL finished;
}

- (void)start:(NSString*)path withArgs:(NSArray*)args;
- (void)finish;
- (BOOL)isProcessForTask:(NSTask*)aTask;
- (BOOL)isFinished;

@end
