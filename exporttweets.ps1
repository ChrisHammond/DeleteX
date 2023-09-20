# This script is used to take the Tweets.js file from your Twitter archive and convert it to a CSV with a list of all TweetIds on separate lines. This then gets used by the DeleteX tool to delete all your tweets.
# Script will not run in some Windows envirments, you can enable by running this in a "run as Adminstrator" Powershell prompt.
# set-executionpolicy remotesigned
# Paths
$tweetJsPath = "tweets.js"
$outputCsvPath = "tweets.csv"

# Read the cleaned tweet.js file content
$content = Get-Content -Path $tweetJsPath -Raw

$content = $content -replace 'window\.YTD\.tweets\.part0\s+=\s+', ''

# Convert the content to a PowerShell object
try {
    $tweets = $content | ConvertFrom-Json

    # Extract the id_str values, sort them in ascending order
    $sortedTweets = $tweets | Sort-Object { [int64] $_.tweet.id_str }

    # Write the sorted tweet IDs to the file
    "TweetID" | Out-File $outputCsvPath -Encoding utf8
    $sortedTweets | ForEach-Object { $_.tweet.id_str } | Out-File $outputCsvPath -Append -Encoding utf8

    Write-Host "Finished writing to $outputCsvPath"
}
catch {
    Write-Host "Error converting JSON: $_"
}
