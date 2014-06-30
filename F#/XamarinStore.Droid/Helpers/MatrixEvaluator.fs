namespace XamarinStore

open Android.Graphics
open Android.Animation

type MatrixEvaluator() =
    inherit Java.Lang.Object()
    let startComponents = Array.init 9 (fun i ->0.0f)
    let endComponents = Array.init 9 (fun i ->0.0f)
    let currentComponents = Array.init 9 (fun i ->0.0f)
    let result = new Matrix ()

    interface ITypeEvaluator with
        member this.Evaluate (fraction, startValue, endValue) =
            (startValue :?> Matrix).GetValues startComponents
            (endValue :?> Matrix).GetValues endComponents

            for i in 0..8 do
                currentComponents.[i] <- startComponents.[i] + (endComponents.[i] - startComponents.[i]) * fraction

            result.SetValues currentComponents
            result :> Java.Lang.Object