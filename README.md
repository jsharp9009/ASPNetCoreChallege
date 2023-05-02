
# Coding Challenge: Create a component that can be added to any ASP.NET core app to handle redirects.

#### Acceptance Criteria

- The component will get the redirects from a RESTful API, but for now create a mock service that returns the following JSON.
- The component must cache the results in memory, but allow multiple threads to read the cache.
- The cache must be updated every few minutes, without requiring any HTTP request to wait for the refresh.
- The cache duration must be configurable.
- Failed API calls are logged as errors
- Successful API calls and cache refreshes are logged as information.
- If useRelative is true, redirects target a relative destination instead of an exact destination. For example, in the data below "/product-directory/bits/masonary/diamond-tip" would redirect to "/products/bits/masonary/diamond-tip".
 

#### Sample service results:

````
[{
        "redirectUrl": "/campaignA",
        "targetUrl": "/campaigns/targetcampaign",
        "redirectType": 302,
        "useRelative": false
    }, {
        "redirectUrl": "/campaignB",
        "targetUrl": "/campaigns/targetcampaign/channelB",
        "redirectType": 302,
        "useRelative": false
    }, {
        "redirectUrl": "/product-directory",
        "targetUrl": "/products",
        "redirectType": 301,
        "useRelative": true
    }
]

````


