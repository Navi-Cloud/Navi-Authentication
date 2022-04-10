package todoapp

import (
    "dagger.io/dagger"
    "universe.dagger.io/bash"
    "universe.dagger.io/docker"
)

dagger.#Plan & {
    client: {
        filesystem: {
            "./": read: {
                contents: dagger.#FS
                exclude: [".idea"]
            }
        },
        network: {
            "unix:///var/run/docker.sock": {
                connect: dagger.#Socket
            }
        }
    }
    actions: {
        deps: {
            mainContainer: docker.#Build & {
                steps: [
                    docker.#Pull & {
                        source: "mcr.microsoft.com/dotnet/sdk:6.0"
                    },
                    
                    bash.#Run & {
                        workdir: "/"
                        script:  contents: #"""
                            apt-get update
                            DEBIAN_FRONTEND='noninteractive' apt-get install -y --no-install-recommends ca-certificates curl gnupg lsb-release
                            curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
                            echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
                            apt-get update
                            DEBIAN_FRONTEND='noninteractive' apt-get install -y --no-install-recommends docker-ce-cli
                            curl -L "https://github.com/docker/compose/releases/download/1.29.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
                            chmod a+x /usr/local/bin/docker-compose
                        """#
                    },

                    docker.#Copy & {
                        contents: client.filesystem."./".read.contents
                        dest: "/src"
                    },

                    bash.#Run & {
                        workdir: "/src"
                        script: contents: #"""
                            dotnet restore
                        """#
                    },
                ]
            }
        }

        unitTest: bash.#Run & {
            input:   deps.mainContainer.output
            workdir: "/src"
            script: contents: #"""
                dotnet test --no-restore NaviAuthUnitTest/NaviAuthUnitTest.csproj
                """#
        }

        integrationTest: bash.#Run & {
            input: deps.mainContainer.output
            mounts: docker: {
                contents: client.network."unix:///var/run/docker.sock".connect
                dest: "/var/run/docker.sock"
            }
            workdir: "/src"
            script: contents: #"""
                docker-compose -f NaviAuthIntegrationTest/docker-compose.yml up -d && sleep 5
                dotnet test --no-restore NaviAuthIntegrationTest/NaviAuthIntegrationTest.csproj
                docker-compose -f NaviAuthIntegrationTest/docker-compose.yml down
            """#
        }

        test: {
            unitTestAction: unitTest
            integrationTestAction: integrationTest
        } 
    }
}