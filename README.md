# Photo Renamer

The .net core console app for study. It will add prefix("created_datetime" and "category") to iOS photos(JPG and HEIC) and movies(MOV)

# Usage

```cmd
> dotnet PhotoRenamer.dll --help
Copyright (C) 2019 PhotoRenamer

  -v, --verbose     Optional. Set output to verbose messages.

  -i, --input       Optional. Set target directry(full path) which has photos to rename.

  -c, --category    Optional. Set category string aiming to identify the instruments taking photos. If this option is
                    not set, current directly name is automatically used.

  -p, --preview     Optinonal. Preview the process, not rename any files.

  --help            Display this help screen.

  --version         Display version information.
```
