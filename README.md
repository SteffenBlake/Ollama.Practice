# Ollama.Practice

## Requirements

### (If on Windows) Windows Subsystem for Linux
Confirm via `wsl --list --verbose`
https://learn.microsoft.com/en-us/windows/wsl/install

**All steps below assume you are doing them inside a linux environment, windows is not actively supported**

### Python 
Confirm via `python --version`
https://www.python.org/downloads/

### .NET 9
Confirm via `dotnet --list-sdks`
https://dotnet.microsoft.com/en-us/download

### Docker
Confirm via `docker --version`
https://docs.docker.com/engine/install/

### Nvidia supported GPU
Confirm via `nvidia-smi`
Check for `CUDA Version: 11` or better
https://www.nvidia.com/en-in/drivers/

### nvidia-container-toolkit
Confirm via `docker run --rm --gpus all nvidia/cuda:12.9.0-base-ubuntu22.04 nvidia-smi`
You should get the exact same output as the prior step (This runs `nvidia-smi` inside a container, confirming docker can see and use your GPU)
https://docs.nvidia.com/datacenter/cloud-native/container-toolkit/latest/install-guide.html

## Running the application
1. Checkout this repo via git: `git clone git@github.com:SteffenBlake/Ollama.Practice.git`
2. `cd` into `./src/Ollama.Practice.AppHost/`
3. `dotnet run`
4. You'll see a log into the terminal of `Login to the dashboard at https://0.0.0.0:17010/login?t=<token>`, open this url in your browser to access the Aspire Dashboard
5. Eventually the `crew` job will display `Finished`, at which point you can check out its logs, or you can navigate to `src/Ollama.Practice.Crew/output/<timestamp>/...` to see the various output text files it produced.


