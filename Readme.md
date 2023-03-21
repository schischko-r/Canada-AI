# Neural Netfork for wildfire cost prediction

> Goal: Creatin an interface for a fire dispatcher. The interface includes a fire map with markers. When clicking on a marker, a request is sent to the server with a neural network that evaluates the potential damage from the fire. After that, the user is presented with a summary of the area and approximate fire damage, as well as recommendations on how to extinguish it.

### Project description:

- Collecting statistical data on forest fires in Canada, reports from fire departments on firefighting expenses.
- Cleaning and transforming data.
- Utilizing Microsoft Azure Cloud Services to create and store the neural network model.
- Creating a user interface with a map.
- Creating a system for requests to the Microsoft Azure server.
- Creating a system for requests to weather APIs and geotags for firefighting recommendations.

## Tools and technologies used:
- ![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
- ![Azure](https://img.shields.io/badge/azure-%230072C6.svg?style=for-the-badge&logo=microsoftazure&logoColor=white)
- ![HTML5](https://img.shields.io/badge/html5-%23E34F26.svg?style=for-the-badge&logo=html5&logoColor=white)
- ![CSS3](https://img.shields.io/badge/css3-%231572B6.svg?style=for-the-badge&logo=css3&logoColor=white)
- ![JavaScript](https://img.shields.io/badge/javascript-%23323330.svg?style=for-the-badge&logo=javascript&logoColor=%23F7DF1E)

### Expected outcomes:
- Developed system for storing, processing, and evaluating extinguishing cost of each wildfire
- Cleaned and transformed data to train a neural network
- Analysis of the environment around the fire and recommendations to the dispatcher regarding extinguishing the fire.
- Developed model for predicting fire damage

## Data used:

- [CWFIS Datamart](https://cwfis.cfs.nrcan.gc.ca/datamart/download/nfdbpnt)
- [Canadian Fire Department Profile](https://www.nfpa.org/News-and-Research/Data-research-and-tools/Emergency-Responders/Canada-Fire-Department-Profile)
- [Forest fires](http://nfdp.ccfm.org/en/data/fires.php)
- [Fifty years of wildland fire science in Canada](https://cdnsciencepub.com/doi/10.1139/cjfr-2020-0314)
- [Wildfire Service](https://www.gov.mb.ca/nrnd/wildfire_program/)
- [Saskatchewan Public Safety Agency](https://gisappl.saskatchewan.ca/Html5Ext/?viewer=wfmpublic)
- [Forest Fire Info Map](https://www.lioapplications.lrc.gov.on.ca/ForestFireInformationMap/index.html?viewer=FFIM.FFIM)
- [Active Fire Data](https://firms.modaps.eosdis.nasa.gov/usfs/active_fire/)
- [FireSmoke Canada](https://firesmoke.ca/)
- [Wildfire Graphs](https://www.ciffc.ca/fire-information/wildfire-graphs)
- [Canadian Wildland Fire Information System](https://www.ciffc.ca/fire-information/maps)
- [OpenWeatherMap](openweathermap.org)
- [Mapbox](https://www.mapbox.com/)