# Step by Step Guide to making a change
A demo of using dns control can be found [here](https://ridleyco-my.sharepoint.com/:v:/g/personal/ssaraswati_willowinc_com/ERcGLfhYeXZBh2_t3KStoH0BAeXLbiNOiuzEe0Q_kgrQJA?e=ZYc9BK) this provides a quick view of how to make a change for anyone familiar with git.

## Step 1
Clone the [AzurePlatform](https://dev.azure.com/willowdev/AzurePlatform/_git/AzurePlatform) repository [https://willowdev@dev.azure.com/willowdev/AzurePlatform/_git/AzurePlatform]. This guide uses [Visual studio code](https://code.visualstudio.com/Download) as it provides everything needed to make a change however any git tool will work fine.


![](guide_clone.png)

## Update zone file

Once the repository is cloned and you have opened the folder with Visual Studio Code navigate to DNS/src/zones

![](guide_zones.png)


You will find both willowInc and willowRail zones here. Each file has both a cloudflare and azure section in it

![](guide_sections.png)


The files are organized by general purpose of entries. Lines starting with // are comments and can be used to describe the purpose of an entry or block of entries.


Entry formats can be found inside Dns/src/definitions/dnscontrol.d.ts with a full explanation provided on the [DNS Control docs](https://stackexchange.github.io/dnscontrol/js)

Make your change by adding/removing or modifying an entry.

## Stage and commit your changes


Once you have made all the changes you require you can create a new branch to commit the changes against.

![](guide_branch.png)


And stage them
![](guide_stage.png)

Once staged publish the changes to Azure with

![](guide_publish.png)


## Create a Pull Request

From azure devops we can create a pull request

https://dev.azure.com/willowdev/AzurePlatform/_git/AzurePlatform/pullrequests?_a=mine

![](guide_pr.png)

Select the branch you created and publish and fill out the dns template describing your changes

![](guide_pr2.png)


## Pull Request Preview

You can either create the pull request as a draft if your not ready to make the change yet and want to get feedback or preview the changes or publish it immediately

The pull request will post a msg detailing what the changes would result in. Review this to make sure the changes match what you indented and no additional deletes or adds would be created.

![](guide_preview.png)


Once the pull request is merged and deployed a member of the [DNS manager team](https://dev.azure.com/willowdev/AzurePlatform/_settings/teams?subjectDescriptor=vssgp.Uy0xLTktMTU1MTM3NDI0NS0zMjI3NTczNjA0LTI0ODA5MzAzNzQtMjk5NjM2NjQ4Mi0yNjIyNDk5MzIwLTEtOTkwODgzOTE2LTI5Mzg3MzgyNTItMjE5Mjg0Mjg3Ny05Mzk3MDg5NTc) can deploy it to production by following the steps in the release page
