# DiscogsInsight

.NET Maui Blazor application designed for getting insights into your discogs collection data. 

_________________________________________________________________________________________________________________________________________________________________________________________________________

Uses a discogs username to fetch information about a users discogs collection and stores information locally in an SQLite database. 
Over time the application will become quicker as some data is fetched and stored locally after the initial seed, as required (such as navigating to a page for a specific Artist).

_________________________________________________________________________________________________________________________________________________________________________________________________________

Architecture of the application (layered architecture from view to database)
<br />
- Discogs Insight - is the main MAUI project - contains all UI/Blazor items, view components, wwwroot, etc.
- DiscogsInsight.Service - View logic
- DiscogsInsight.Service.Models
- DiscogsInsight.Service.Tests
- DiscogsInsight.DataAccess
- DiscogsInsight.ApiIntegration
- DiscogsInsight.ApiIntegration.Contract - Api layer interfaces
- DiscogsInsight.ApiIntegration.Models - Model layer
- DiscogsInsight.DataAccess.Models
- DiscogsInsight.DataAccess.Tests
- DiscogsInsight.DataAccess.Contract
- DiscogsInsight.Database - Database models, services, contract


_________________________________________________________________________________________________________________________________________________________________________________________________________

Note: attached photos may not be current.

You will need a discogs username for fetching an initial collection. This is entered when you first open the application, or via the settings page to change it.

![image](https://github.com/BarnacleJones/DiscogsInsight/assets/88416885/37cc2471-93d0-4f0b-8ea6-d6d95089627b)
_________________________________________________________________________________________________________________________________________________________________________________________________________

Once this is entered your basic collection data will be seeded to the database and you can then use the navigation menu to explore:

![image](https://github.com/BarnacleJones/DiscogsInsight/assets/88416885/154381a8-51b4-4395-9c7d-e7d6704ff3e1)

_________________________________________________________________________________________________________________________________________________________________________________________________________
Home page - indicates how much full collection data has been retrieved - the more data, the more accurate the insights are:

![image](https://github.com/BarnacleJones/DiscogsInsight/assets/88416885/8bdf6f69-302f-4cb1-b246-d8a8827aa5b0)

_________________________________________________________________________________________________________________________________________________________________________________________________________
Sample release page:

![image](https://github.com/BarnacleJones/DiscogsInsight/assets/88416885/89c0cc4b-9a79-4666-89bf-123c93ff9ccf)

_________________________________________________________________________________________________________________________________________________________________________________________________________
Work in progress with many features still to implement.
[Trello Board](https://trello.com/b/1RoYKrOK/discogsinsight)




