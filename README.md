# F# Azure Function : Meta pairs from a web page head

This is my first real F# project - something I thought would be a minor stretch. I've done this type of work in C# but wanted to hone my chops with F# and work through getting serverless debug working locally. As it turns out the latter was more of a grunt than I expected.

## Humble Beginnings

The objective of my first F# venture is to *not* stray too far from examples, and blend them in some territory that's familiar to me in other languages. Here I want to use FSharp.Data to retrieve "meta" elements out of the head of a web page using the [HTML Parser](https://fsprojects.github.io/FSharp.Data/library/HtmlParser.html). The result would return name/property elements as keys and their respective content fields as the values in a simple, single JSON object. I'm borrowing from the "hello world" boilerplate F# function (generated from the template) and adding a few blocks of code from the FSharp.Data sample, but am encountering a few points of friction at the first point of departure - trying implement a tuple function to populate the key/value pairs out of the meta elements in the page.

## Example Request

```html
http://localhost:7071/api/gethtmlmeta?url=https://h3tech.dev/post/smart-bars-iot/
```

## Desired JSON output

```json
{
    "author": "Houston Haynes",
    "description": "Teasing features out of an Arduino embedded project using Android and Google Location Services APIs",
    "twitter:card": "summary_large_image",
    "twitter:image": "https://h3tech.dev/img/H3_og_wide_HeliosBars.png",
    "twitter:image:type": "image/png",
    "twitter:title": "Toward Smarter Bicycle Handlebars",
    "_comment": "...and so on and so forth for all meta elements in the page..."
}
```

## The Dev Container

I will spare readers another online digression about how F# is a second-class citizen in the Azure Function App ecosystem. If you're reading this it's likely your already familiar with that long-standing issue. I'll simply jump ahead to how I was able to get a local debug environment working, with its limitations. 

As you can see from the devcontainer.json I added the REST client extension so that the HTTP test file can be used for debugging. I haven't set up a bash to pull the core tools and libraries necessary, but they're listed below. My default container image is Ubuntu 20.

```bash
wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install azure-functions-core-tools-3
dotnet add package FSharp.Data
dotnet add package System.Text.Json
```

The good news is that this provides direct feedback for sample requests.

![Image of WIP](Screenshot_2021-05-09.png)

The bad news is that there's no ionide in Linux. It's a bit old-fashioned but the debugger will give back errors at runtime. I also run *another* editor instance locally so that I can spy changes to the code. Sometime I use Visual Studio, other times I've opened a new VSCode window - it's not something I've settled on.

## The Current Challenge

I've been able to lean on the HTML Parser sample code to provide a quick mental picture of what I'm trying to accomplish. However, with *meta* elements it's a matter of collecting the values out of two different tags. When I try to shift the function to processing a tuple for both the "x" and "y" values the compiler complains about it not matching to the HtmlNode type. So this feels like either I'm missing some syntactic sugar to make this work based on the example, or there's another API or different approach with the HTML Parser that I need to consider.

```fsharp
let links = 
    results.Descendants ["meta"]
    |> Seq.choose (fun (x, y) ->  // trying to pass two values into tuple
            x.TryGetAttribute("name")
            y.TryGetAttribute("content") // this is what I'm thinking
            |> Option.map (fun (a, b) -> x.Value(), y.Value())
    )
    |> Seq.toList
```

```
C:\repo\fsharp_webparser\gethtmlmeta.fs(52,36): error FS0001: This expression was expected to have type 'HtmlNode' but here has type ''a * 'b' [C:\repo\fsharp_webparser\FunctionsInFSharp.fsproj]
```

## The Ask

So - if anyone has ideas on how/where to approach solving for this, I'd appreciate any helpful hint. I'll update this as I go along. Thanks!