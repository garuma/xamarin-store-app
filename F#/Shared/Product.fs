module Product

    open System
    open Helpers

    type ProductType =
    | CSharpShirt = 0
    | FSharpShirt = 2
    | Monkey = 1

    type ProductColor = 
        { Name : string
          ImageUrls : string array }

        override this.ToString() = this.Name
    
    [<CustomEquality; NoComparison>]
    type ProductSize = 
        { Name : string
          Description : string }

        override this.ToString() = this.Description
        override this.Equals(other) =
           match other with
           | :? ProductSize as size -> size.Name = this.Name
           | _ -> false
        override this.GetHashCode() = this.Name.GetHashCode()
    

    type Product = 
        { Price : double
          Size : ProductSize
          Color : ProductColor
          ProductType : int
          Name : string
          Description : string 
          Colors : ProductColor array
          Sizes : ProductSize array }

        //Initializes a product with all unspecified fields set to empty
        static member CreateProduct(?Price, ?Size, ?Color, ?ProductType, ?Name, ?Description, ?Colors, ?Sizes) = 
            let initMember x = Option.fold (fun state param->param) <| x
            { Price = initMember 0.0 Price
              Size = initMember {Name=""; Description=""} Size
              Color = initMember {Name=""; ImageUrls=[||] } Color
              ProductType = initMember 0 ProductType
              Name = initMember "" Name
              Description = initMember "" Description
              Colors = initMember [||] Colors
              Sizes = initMember [||] Sizes}

        member this.GetImageUrls() =
            this.Colors |> Array.collect (fun x->x.ImageUrls)

        member this.GetImageUrl() = //TODO: Cleanup this implementation - can't directly reference `this` in the lambda since it's causes a `self-referencing loop` error
            let index = ref -1
            (fun colors ()-> match colors |> Array.collect (fun x->x.ImageUrls) with
                             | [||] -> ""
                             | urls -> if !index = -1 then index := NextRandom(urls.Length)
                                       urls.[!index])(this.Colors)
            

        static member ImageForSize(url, (width:float32)) =
            sprintf "%s?width=%g" url width

        member this.ImageForSize width =
            Product.ImageForSize(this.GetImageUrl()(), width)

        member this.GetPriceDescription () =
            if this.Price < 0.01 then "Free" else this.Price.ToString("C")

        member this.CreateCopy() =
            { this with Colors = [| this.Color |]; Description = ""; Sizes = [||]}