//
//  ShareRecUnity3DExtension.m
//  ShareRecGameSample
//
//  Created by vimfung on 14-11-14.
//  Copyright (c) 2014年 掌淘科技. All rights reserved.
//

#import "ShareRecUnity3DExtension.h"
#import <ShareREC/ShareREC.h>
#import <ShareREC/ShareREC+Ext.h>
#import "JSONKit.h"
#import "NSGIF.h"
#import "NSLivePhoto.h"
#import <AssetsLibrary/AssetsLibrary.h>
#import <Photos/Photos.h>

#if defined (__cplusplus)
extern "C" {
#endif
    
    /**
     *	@brief	开始录制
     */
    extern void __iosShareRECStartRecording();
    
    /**
     *	@brief	结束录制
     *
     *  @param  observer    观察回调对象名称
     */
    extern void __iosShareRECStopRecording (void *observer);
    
    /**
     *	@brief	播放最后一个录像
     */
    extern void __iosShareRECPlayLastRecording ();
    
    extern void __iosSaveVideoToGif (int quility);
    extern void __iosSaveVideoToLivePhoto ();
    /**
     *	@brief	设置码率，默认为800kbps = 800 * 1024
     *
     *	@param 	bitRate 	码率
     */
    extern void __iosShareRECSetBitRate (int bitRate);
    
    /**
     *	@brief	设置帧率
     *
     *	@param 	fps 	帧率
     */
    extern void __iosShareRECSetFPS (int fps);
    
    /**
     *	@brief	设置最短录制时间，默认4秒
     *
     *	@param 	time    时间，0表示不限制
     */
    extern void __iosShareRECSetMinimumRecordingTime(float time);
    
    /**
     *	@brief	获取最后一个录像的路径
     */
    extern const char* __iosShareRECLastRecordingPath ();
    
    /**
     *  编辑最后一个录像
     *
     *  @param title    标题
     *  @param userData 用户数据
     *  @param observer 回调对象名称
     */
    extern void __iosShareRECEditLastRecording (void *title, void *userData, void *observer);
    extern void __iosSetClipbord (void *txt);

    /**
     *  编辑最后一个录像
     *
     *  @param observer 回调对象名称
     */
    extern void __iosShareRECEditLastRecordingNew (void *observer);
    
    /**
     *  设置是否同步录入语音解说
     *
     *  @param syncAudioComment 同步语音解说标识，YES 表示同步录入, NO 表示不录入。
     */
    extern void __iosShareRECSetSyncAudioComment (bool syncAudioComment);
    
    extern bool __iosCanOpenUrl (void *url);
    
#if defined (__cplusplus)
}
#endif

#if defined (__cplusplus)
extern "C" {
#endif
    
    extern void UnitySendMessage(const char* obj, const char* method, const char* msg);
    
    void __iosShareRECStartRecording()
    {
        [ShareREC startRecording];
    }
    
    void __iosShareRECStopRecording (void *observer)
    {
        
        NSString *observerStr = nil;
        if (observer)
        {
            observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        }
        
        [ShareREC stopRecording:^(NSError *error) {
            
            NSMutableDictionary *resultDict = [NSMutableDictionary dictionaryWithDictionary:@{@"name" : @"StopRecordingFinished"}];
            if (error)
            {
                NSDictionary *errDict = @{@"code" : @(error.code), @"message" : error.localizedDescription ? error.localizedDescription : @""};
                [resultDict setObject:errDict forKey:@"error"];
            }
            
            NSString *resultStr = [resultDict JSONString];
            UnitySendMessage([observerStr UTF8String], "shareRECCallback", [resultStr UTF8String]);
            
        }];
    }
    
    void __iosSaveVideoToGif (int quility){
        //播放视频应该没有需求，改为保存gif
        NSString *urlStr = [ShareREC lastRecordingPath];
        if (urlStr)
        {
            [NSGIF optimalGIFfromURL:[NSURL fileURLWithPath:urlStr] loopCount:0 quility:quility completion:^(NSURL *GifURL) {
                //delete video
                NSFileManager *fileManager = [NSFileManager defaultManager];
                [fileManager removeItemAtPath:urlStr error:nil];
                
                //save album
                NSData * gifData = [NSData dataWithContentsOfURL:GifURL];
                ALAssetsLibrary *library = [[ALAssetsLibrary alloc] init];
                NSDictionary *metadata = @{@"UTI":(__bridge NSString *)kUTTypeGIF};
                [library writeImageDataToSavedPhotosAlbum:gifData metadata:metadata completionBlock:^(NSURL *assetURL, NSError *error) {
                    if (error) {
                        // "保存图片失败"
                    }else{
                        //保存图片成功"
                    }
                    
                    //delete
                    [fileManager removeItemAtPath:[GifURL absoluteString] error:nil];
                }] ;
            }];
        }
    }
    
    void __iosSaveVideoToLivePhoto (){
        NSString *homeDir = NSHomeDirectory();
        NSString *tmpDir = [homeDir stringByAppendingPathComponent:@"tmp"];
        NSString * outImagePath = [tmpDir stringByAppendingPathComponent:[NSString stringWithCString:"lp"]];
        outImagePath = [outImagePath stringByAppendingPathExtension:@"jpg"];
        
        NSString * outVideoPath = [tmpDir stringByAppendingPathComponent:[NSString stringWithCString:"lp"]];
        outVideoPath = [outVideoPath stringByAppendingPathExtension:@"mov"];
        
        NSString *mp4Path = [ShareREC lastRecordingPath];
        NSURL *videoPath = [NSURL fileURLWithPath:mp4Path];
        NSURL *outVideoPathUrl = [NSURL fileURLWithPath:outVideoPath];
        NSURL *outImagePathUrl = [NSURL fileURLWithPath:outImagePath];
        
        NSFileManager *fileManager = [NSFileManager defaultManager];
        [fileManager removeItemAtPath:outImagePath error:nil];
        [fileManager removeItemAtPath:outVideoPath error:nil];
        
        if (mp4Path)
        {
            //首先保存封面图
            AVURLAsset *asset = [[AVURLAsset alloc] initWithURL:videoPath options:nil];
            AVAssetImageGenerator *gen = [[AVAssetImageGenerator alloc] initWithAsset:asset];
            gen.appliesPreferredTrackTransform = YES;
            gen.requestedTimeToleranceAfter = kCMTimeZero;// 精确提取某一帧,需要这样处理
            gen.requestedTimeToleranceBefore = kCMTimeZero;// 精确提取某一帧,需要这样处理
            CMTime time = CMTimeMakeWithSeconds(0, 600);
            NSError *error = nil;
            CMTime actualTime;
            CGImageRef image = [gen copyCGImageAtTime:time actualTime:&actualTime error:&error];
            UIImage *img = [[UIImage alloc] initWithCGImage:image];
            CMTimeShow(actualTime);
            CGImageRelease(image);
            
            NSData *data;
            data = UIImageJPEGRepresentation(img,1.0);
            [data writeToFile:outImagePath atomically:FALSE];
            
            //转换mp4为mov格式视频
            AVAssetExportSession *exportSession = [[AVAssetExportSession alloc] initWithAsset:asset presetName:AVAssetExportPresetHighestQuality];
            exportSession.outputURL = outVideoPathUrl;
            exportSession.outputFileType = AVFileTypeQuickTimeMovie;
            exportSession.shouldOptimizeForNetworkUse= NO;
            [exportSession exportAsynchronouslyWithCompletionHandler:^(void)
             {
                 switch (exportSession.status) {
                     case AVAssetExportSessionStatusCancelled:
                         break;
                     case AVAssetExportSessionStatusUnknown:
                         break;
                     case AVAssetExportSessionStatusWaiting:
                         break;
                     case AVAssetExportSessionStatusExporting:
                         break;
                     case AVAssetExportSessionStatusCompleted:
                         NSLog(@"AVAssetExportSessionStatusCompleted");
                     {
                         [fileManager removeItemAtPath:mp4Path error:nil];
                         //保存为live photo
                         [[[NSLivePhoto alloc] init] saveLivePhoto:outImagePathUrl video:outVideoPathUrl];
                     }
                         
                         break;
                     case AVAssetExportSessionStatusFailed:
                         break;
                 }
             }];
        }
    }
    void __iosShareRECPlayLastRecording ()
    {
       [ShareREC playLastRecording];
    }
    
    void __iosShareRECSetBitRate (int bitRate)
    {
        [ShareREC setBitRate:bitRate];
    }
    
    void __iosShareRECSetFPS (int fps)
    {
        [ShareREC setFPS:fps];
    }
    
    void __iosShareRECSetMinimumRecordingTime(float time)
    {
        [ShareREC setMinimumRecordingTime:time];
    }
    
    const char* __iosShareRECLastRecordingPath ()
    {
        NSString *urlStr = [ShareREC lastRecordingPath];
        if (urlStr)
        {
            // 这里保存视频到相册，同时删除沙盒下的视频
            if (UIVideoAtPathIsCompatibleWithSavedPhotosAlbum(urlStr)) {
                UISaveVideoAtPathToSavedPhotosAlbum(urlStr, nil,nil, nil);
            }

            return strdup([[ShareREC lastRecordingPath] UTF8String]);
        }
        
        return strdup([@"" UTF8String]);
    }
    
    void __iosShareRECEditLastRecording (void *title, void *userData, void *observer)
    {
        NSString *titleStr = nil;
        if (title)
        {
            titleStr = [NSString stringWithCString:title encoding:NSUTF8StringEncoding];
        }
        
        NSDictionary *userDataDict = nil;
        if (userData)
        {
            NSString *userDataStr = [NSString stringWithCString:userData encoding:NSUTF8StringEncoding];
            userDataDict = [userDataStr objectFromJSONString];
        }
        
        NSString *observerStr = nil;
        if (observer)
        {
            observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        }
        
        [ShareREC editLastRecordingWithTitle:titleStr userData:userDataDict onClose:^{
            
            NSDictionary *resultDict = @{@"name" : @"SocialClose"};
            NSString *resultStr = [resultDict JSONString];
            UnitySendMessage([observerStr UTF8String], "shareRECCallback", [resultStr UTF8String]);
            
        }];
    }
    
    
    void __iosSetClipbord (void *txt)
    {
        NSString *txtStr = nil;
        if (txt)
        {
            txtStr = [NSString stringWithCString:txt encoding:NSUTF8StringEncoding];
        }
        
        [UIPasteboard generalPasteboard].string = txtStr;
    }
    
    
    void __iosShareRECEditLastRecordingNew (void *observer)
    {
        NSString *observerStr = nil;
        if (observer)
        {
            observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        }
        
        [ShareREC editLastRecording:^(BOOL cancelled) {
           
            NSDictionary *resultDict = @{@"name" : @"EditResult", @"cancelled" : @(cancelled)};
            NSString *resultStr = [resultDict JSONString];
            UnitySendMessage([observerStr UTF8String], "shareRECCallback", [resultStr UTF8String]);
            
        }];
    }
    
    void __iosShareRECSetSyncAudioComment (bool syncAudioComment)
    {
        [ShareREC setSyncAudioComment:syncAudioComment ? YES : NO];
    }
    
    bool __iosCanOpenUrl(void* url){
        NSURL *nsurl = nil;
        if (url)
        {
            nsurl = [NSURL URLWithString:[NSString stringWithCString:url encoding:NSUTF8StringEncoding]];
            if ([[UIApplication sharedApplication] canOpenURL:nsurl]) {
                //能打开淘宝就打开淘宝
               // [[UIApplication sharedApplication] openURL:nsurl];
                return true;
            } else {
                return false;
            }
        }
        return false;
    }
    
#if defined (__cplusplus)
}
#endif

@implementation ShareRecUnity3DExtension

@end
