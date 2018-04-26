#!/bin/bash
dotnet test Toggl.Foundation.Tests --no-build --filter DisplayName~$1
