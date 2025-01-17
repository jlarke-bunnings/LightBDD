Define-Step -Name 'Update version' -Target 'all,build' -Body {
    function Replace-InFile([string]$file, [string]$version, [string[]]$matchings)
    {
        Write-ShortStatus "Updating $file with $version..."
        $content = Get-Content $file -Encoding UTF8
        foreach($match in $matchings)
        {
            $from = [Regex]::Escape($match) -replace '%','[0-9]+(\.[0-9]+)*'
            $to = $match -replace '%',$version

            $content = $content -replace $from,$to
        }
        Set-Content $file -Value $content -Encoding UTF8
    }

    $version = (Get-Content 'make\current_version').Trim()
    Write-ShortStatus "Updating version to $version..."

    Replace-InFile 'AssemblyVersion.cs' $version 'Version("%")'
    Replace-InFile 'src\Common.props' $version '<VersionPrefix>%</VersionPrefix>'
    Replace-InFile 'QuickStart.txt' $version 'version %!'
    Replace-InFile 'templates\LightBDD.VSIXTemplates\source.extension.vsixmanifest' $version 'Identity Id="fe1ca06b-95ef-41a1-8e09-fe903b2945a4" Version="%"'
    Get-ChildItem '.\templates' 'ProjectTemplate.csproj' -Recurse | %{
        Replace-InFile $_.FullName $version 'Include="LightBDD.Fixie2" Version="%"','Include="LightBDD.NUnit2" Version="%"','Include="LightBDD.NUnit3" Version="%"','Include="LightBDD.XUnit2" Version="%"','Include="LightBDD.MsTest2" Version="%"'
    }
}

Define-Step -Name 'Build' -Target 'all,build' -Body {
    call dotnet build --configuration Release /nologo /p:TreatWarningsAsErrors=true /m
}

Define-Step -Name 'Tests' -Target 'all,test' -Body {
    call dotnet test --nologo --no-build --configuration Release
}

Define-Step -Name 'Pack' -Target 'all,pack' -Body {
    Remove-Item 'output' -Force -Recurse -ErrorAction SilentlyContinue | Out-Null
    call dotnet pack --configuration Release /nologo /p:TreatWarningsAsErrors=true /m
}

Define-Step -Name 'Prepare templates' -Target 'all,pack' -Body {

    Get-ChildItem '.\templates' -Recurse -Filter '*.vstemplate' | %{
    Write-Host $_
        $templateDirectory = $_.Directory.FullName
        Write-ShortStatus "Processing: $templateDirectory"
        Copy-Item 'logo\lightbdd.ico' -Destination "$templateDirectory\lightbdd.ico" | Out-Null
    }

    Get-ChildItem '.\templates' -Recurse -Filter '*.vsixmanifest' | %{
        $templateDirectory = $_.Directory.FullName
        Write-ShortStatus "Processing: $templateDirectory"
        Copy-Item 'logo\lightbdd_128.png' -Destination "$templateDirectory\lightbdd_128.png" | Out-Null
        Copy-Item 'logo\lightbdd_32.ico' -Destination "$templateDirectory\lightbdd_32.ico" | Out-Null
    }
}

Define-Step -Name 'Build VSIX package' -Target 'all,pack' -Body {
    call "msbuild.exe" templates\LightBDD.VSIXTemplates.sln /tv:current /t:"Clean,Build" /p:Configuration=Release /m /verbosity:m /nologo /p:TreatWarningsAsErrors=true /nr:false
    copy-item 'templates\LightBDD.VSIXTemplates\bin\Release\LightBDD.VSIXTemplates.vsix' -Destination 'output'
}

Define-Step -Name 'Build Examples' -Target 'all,build' -Body {
    call dotnet build examples --configuration Release /nologo /p:TreatWarningsAsErrors=true /m /p:GeneratePackageOnBuild=false
}