### Split Building Limits Boilerplate

Included in this project is some boilerplate code to get started on the programming task. You may of course alter any of the boilerplate code to fit your solution.

`samples` 

Includes [GeoJSON](https://en.wikipedia.org/wiki/GeoJSON) sample inputs for the building limits and height plateaus.


`Program.cs`

Includes methods to read GeoJSON from a local file, deserialize to a [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) feature collection
and extract the relevant geometries.

Be sure to update the file path constants before running the program.

`SplitBuildingLimitsClass.cs`

This is where you are expected to work from!

### Requirements -- Architecture terminology
- Site: An area in which you will construct buildings, parks, roads.
- Building limits: Areas (polygons) on your site where you are allowed to build. They 
will cover a subset of the site. 
- Height plateaus: Areas (polygons) on your site with different elevations. Your site is 
a continuous irregular terrain, but before building, you level your terrain into discrete 
plateaus with constant elevation. These cover the whole site.

- In this scenario, a user has set up a site with corresponding building limits and height 
plateaus. We run a preprocessing step that splits the building limit into polygons 
corresponding to the height plateaus. These building limit polygons should have the 
elevation of the corresponding height plateau set as a property

### Validation
- Height plateaus (HP) intersecting one another
    - Thoughts: 
        - Return Error for intersecting HP. As it may results in ambiguous area to the same space which may need further validation from the user
    - Action:
        - Throw an invalid input exception when overlapped area on HP input detected.
- Building limits (BL) intersecting one another 
    - Thoughts:
        - Building limits itself doesn't result ambiguous for overlapped areas from input sequences, we may just merge the area for further processing
    - Action:
        - Execute a union operation to the input building limits list
- Height plateaus not completely covering the building limits 
    - Thoughts:
        - There might be two ways to handle the case:
            - 1) Return an Error and let the user to fix the issue. If every single area of the building limits has to have the HP feature.
            - 2) Give a Warning to the user, but still be able to proceed. For those building limits without HP, either give a default value to flag it or mention in the notes that results may contain BL without HP feature.
    - Action in this test:
        - Execute method 2) logic
        - Step 1: Give a warning to the user that some BL is outside of the provided HP area, a default value (maybe 9999.f) is set to those BLs ==> May further discussed if it makes sense in the construction industry or there might be an existing domain logic to handle this already
        - Step 2: Initialize the BL area to the default value, and updates those within the provided HP area.

### Concurrency
- Case study: Imagine that two users, Bob, and Mary, make modifications to the same project. Bob 
makes some modifications to the building limits while Mary changes the height 
plateaus. What happens if they call the API at the same time? Make sure the API 
deals with concurrent updates.
- Thoughts: 
    - Modifications to the inputs concurrently will result in different results obtained
    - To avoid this happening, we may add a lock to the API call

### Building limits spliting process
- Thoughts:
    - Brute Force way: Nested Loop
        - 1) Outer loop: Iterate through the BL input list
        - 2) Inner loop: Iterate through the HP input list
        - 3) Operations: For those BL and HP has overlapped area, execute a polygon intersection operation to get the result BL polygon and assign the HP elevation feature to the result, add the new polygon (with HP feature) to the result list
            - Q1: Is there an existing API can be used to do the polygon intersection operation?
            - Q2: What are the special cases may have for the intersection results and how to handle these special cases (May need to check the API, see how its got defained or handled)
                - (1) Polygon intersects with shared boundary only ==> Not valid intersection in our case
                - (2) Polygon intersects with shared corner point only ==> Not valid intersection in our case
            - Q3: How to handle the remainer area of the original BL after the intersection operation
    - Slightly optimized way:
        - Run a nested loop to construct a mapping table with giving each BL the list of HPs has overlapped area to the BL
        - Do intersection operation in parallel for each pair in the mapping table and combine the results afterwards

### Testing
- Unit tests
    - Polygon intersection operation tests
    - Error handling tests for invalid inputs
- Functional tests
    - API calling tests with valid/invalid inputs
- Performance tests
    - Concurrent calling the API with modifications to the input