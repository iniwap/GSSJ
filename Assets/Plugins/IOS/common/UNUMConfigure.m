//
//  RNUMConfigure.m
//  UMComponent
//
//  Created by wyq.Cloudayc on 14/09/2017.
//  Copyright © 2017 Facebook. All rights reserved.
//

#import "UNUMConfigure.h"

@implementation UNUMConfigure

+ (void)initWithAppkey:(NSString *)appkey channel:(NSString *)channel
{
  SEL sel = NSSelectorFromString(@"setWraperType:wrapperVersion:");
  if ([UMConfigure respondsToSelector:sel]) {
    [UMConfigure performSelector:sel withObject:@"Unity" withObject:@"3.0"];
  }
  [UMConfigure initWithAppkey:appkey channel:channel];
}
@end
