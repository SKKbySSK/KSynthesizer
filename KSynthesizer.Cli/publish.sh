dotnet publish --runtime linux-arm --self-contained true --output publish/
rsync -r publish/ pi@raspberrypi.local:~/KSynthesizer.Cli/
