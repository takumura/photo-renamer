# Photo Renamer

The .net core console app for study. It will add prefix("created_datetime" and "category") to iOS photos(JPG and HEIC) and movies(MOV)

# Usage

```cmd
> .\PhotoRenamer.Console.exe help
Usage:  [options...]

Options:
  -i, --input <String>       Optional. Set target directry(full path) which has photos to rename. (Default: )
  -c, --category <String>    Optional. Set category string aiming to identify the instruments taking photos. If this option is not set, current directly name is automatically used. (Default: )
  -v, --verbose              Optional. Set output to verbose messages. (Optional)

Commands:
  help       Display help.
  preview    Preview the rename process and display the expected result
  version    Display version.
```
