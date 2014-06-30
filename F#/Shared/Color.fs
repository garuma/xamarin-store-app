module Color

#if __IOS__
open MonoTouch.UIKit
open MonoTouch.CoreGraphics
#endif


type Color = 
    { R : double
      G : double
      B : double }
    static member FromHex hex =
        let at offset = double ((hex >>> offset) &&& 0xFF)
        { R = at 16 /255.0
          G = at 8 /255.0
          B = at 0 /255.0 }

	#if __IOS__
    member this.ToUIColor () =
        UIColor.FromRGB (float32 this.R, float32 this.G, float32 this.B)
	#endif

	#if __ANDROID__
    member this.ToAndroidColor () =
        Android.Graphics.Color.Rgb (int (255.0 * this.R), int (255.0 * this.G), int (255.0 * this.B))
	#endif

let Purple =    Color.FromHex 0xB455B6
let Blue =      Color.FromHex 0x3498DB
let DarkBlue =  Color.FromHex 0x2C3E50
let Green =     Color.FromHex 0x77D065
let Gray =      Color.FromHex 0x738182
let LightGray = Color.FromHex 0xB4BCBC