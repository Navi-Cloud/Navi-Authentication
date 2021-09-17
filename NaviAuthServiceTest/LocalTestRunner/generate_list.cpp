#include "excludeFileList.h"

string createTestCommand() {
    string target = "\"";
    for_each(excludeCoverage.begin(), excludeCoverage.end(), [&](string eachExclusion) {
        target += eachExclusion + "%2c";
    });

    // Remove last 3 character, since it is abundant character.
    for (int i = 0; i < 3; i++) {
        target.pop_back();
    }

    // Add Last quote.
    target += "\"";

    return target;
}

int main(void) {
    cout << createTestCommand() << endl;
    return 0;
}