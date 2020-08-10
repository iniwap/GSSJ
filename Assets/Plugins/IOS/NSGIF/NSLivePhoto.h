//
//  NSLivePhoto.h
//  Unity-iPhone
//
//  Created by 王廷选 on 2018/7/19.
//

#ifndef NSLivePhoto_h
#define NSLivePhoto_h

#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>
#import <Photos/Photos.h>
#import <CoreMedia/CMMetadata.h>
#import <MobileCoreServices/MobileCoreServices.h>

@interface NSLivePhoto : NSObject

- (void)saveLivePhoto:(NSURL *)photoURL video:(NSURL *)videoURL;
- (void)useAssetWriter:(NSURL *)photoURL video:(NSURL *)videoURL identifier:(NSString *)identifier complete:(void (^)(BOOL success, NSString *photoFile, NSString *videoFile, NSError *error))complete ;
- (void)addMetadataToPhoto:(NSURL *)photoURL outputFile:(NSString *)outputFile identifier:(NSString *)identifier ;
- (void)addMetadataToVideo:(NSURL *)videoURL outputFile:(NSString *)outputFile identifier:(NSString *)identifier ;
- (void)writeTrack:(NSInteger)trackIndex ;
- (void)finishWritingTracksWithPhoto:(NSString *)photoFile video:(NSString *)videoFile complete:(void (^)(BOOL success, NSString *photoFile, NSString *videoFile, NSError *error))complete ;
- (AVMetadataItem *)createContentIdentifierMetadataItem:(NSString *)identifier ;
- (AVAssetWriterInput *)createStillImageTimeAssetWriterInput ;
- (AVMetadataItem *)createStillImageTimeMetadataItem ;
- (NSString *)filePathFromDoc:(NSString *)filename ;
- (void)deleteFile:(NSString *)file ;

@end

#endif /* NSLivePhoto_h */
