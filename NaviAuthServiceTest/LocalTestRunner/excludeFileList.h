#include <iostream>
#include <vector>
#include <fstream>
#include <algorithm>

using namespace std;


// Exclusion Folder, Exclusion Files.
// May include wildcard expression, such as *[stars]
vector<string> excludeCoverage {
    "**/sns-proto/**",
    "**/Models/**",
    "**/Program.cs",
    "**/Startup.cs"
};