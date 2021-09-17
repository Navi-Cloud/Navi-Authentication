#include "excludeFileList.h"

string createTestCommand() {
    string baseTestCommand = "dotnet test -p:CoverletOutputFormat=cobertura -p:CollectCoverage=true -p:ExcludeByFile=\"";
    for_each(excludeCoverage.begin(), excludeCoverage.end(), [&](string eachExclusion) {
        baseTestCommand += eachExclusion + "%2c";
    });

    // Remove last 3 character, since it is abundant character.
    for (int i = 0; i < 3; i++) {
        baseTestCommand.pop_back();
    }

    // Add Last quote.
    baseTestCommand += "\"";

    return baseTestCommand;
}

int main(void) {
    // A Shell Script to run 
    vector<string> baseCommand {
        "#!/bin/bash",
        "dotnet clean",
        "dotnet restore",
        createTestCommand(),
        "reportgenerator -reports:coverage.cobertura.xml -targetdir:\"testreport\" -reporttypes:Html",
        "open testreport/index.html"
    };

    // Open File, but in truncate mode.[Warning that this is NOT IOS::APP constant]
    ofstream ofs("run_test.sh", ios::trunc);

    // Write it
    for_each(baseCommand.begin(), baseCommand.end(), [&](string& eachCommand){
        ofs << eachCommand << endl;
    });
    ofs.close();

    return 0;
}