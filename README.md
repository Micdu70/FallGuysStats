# Fall Guys Stats "FE" (Frenchy Edition) par Micdu70
Programme qui permet de récupérer les stats de Fall Guys (via la lecture des logs) pour suivre ce que vous faites en jeu.

INFO: La Version Officielle est disponible ici => https://github.com/ShootMe/FallGuysStats

## Téléchargement
**Dernière version: 1.137**

  - => [FallGuysStats.zip](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/FallGuysStats.zip)

***-OU-***

  - Si votre logiciel antivirus bloque l'utilisation du programme, utilisez alors cette version qui ne possède pas la fonction de MAJ automatique => [FallGuysStatsManualUpdate.zip](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/FallGuysStatsManualUpdate.zip)
  
## Utilisation
  - Extraire le contenu du fichier zip téléchargé vers un nouveau dossier vide
  - Dans ce nouveau dossier, lancer le programme
  - Configurer le programme comme bon vous semble (afficher l'overlay/modifier des options/etc.)
  - Lancer Fall Guys
 
 **IMPORTANT: Les stats des émissions seront enregistrées dans le programme UNIQUEMENT à la fin de celles-ci.**

![Fall Guys Stats](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/mainWindow.png)

![Stats des manches de Fall Guys](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/levelWindow.png)

## Langues disponibles
  - FallGuysStats supporte les langues suivantes :
    - English (Anglais)
    - Français *[langue par défaut du programme]*
    - Korean (Coréen)
    - Japanese (Japonais)
    - Simplified Chinese (Chinois Simplifié)

## Overlay
![Overlay](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/overlay.png)

  - Appuyez sur la touche 'T' pour changer la couleur de l'arrière-plan.
  - Appuyez sur la touche 'F' pour inverser horizontalement l'affichage.
  - Appuyez sur touche 'P' pour passer au profil suivant.
  - Appuyez sur les touches des chiffres situés au dessus des lettres (de '1' à '9') pour choisir le numéro du profil désiré.
  - Maintenez la touche 'Maj' enfoncée et utilisez la molette de votre souris pour changer de profil.
  - Maintenez la touche 'Maj' enfoncée et utilisez la touche directionnelle 'Haut' ou 'Bas' pour changer de profil.
  - Appuyez sur la touche 'C' pour afficher le nombre de joueurs par type de support.
  - Appuyez sur la touche 'R' pour colorer le nom des manches selon leur type.

## Supprimer des émissions ou déplacer des émissions vers un autre profil
![Supprimer une ou plusieurs émissions](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/showsWindow.png)

  - Cliquer pour voir les stats des émissions.
  - Sélectionner une ou plusieurs émissions avec la touche 'Ctrl' ou avec la combinaison de touches 'Ctrl'+'Maj'.
  - Faire un clique-droit sur la sélection pour pouvoir déplacer ou supprimer celle-ci.

## Changelog de la "FE" (Frenchy Edition)
  - 1.137
    - ++++N.B.: Basée sur la Version Officielle 1.136 (datant du 12 février 2023)
	- ++Ajouté: Langue 'Français'
	- ++Ajouté: Type de manche "Logique" et "Invisibeans"
	- +Corrigé: Visuel de l'interface utilisateur dans la fenêtre de configuration
	- +Corrigé: Langue 'English'
	- +Corrigé: Filtres 'Parties'
	- +Corrigé: Ordre et enregistrement de la configuration des filtres
	- +Corrigé: Détection des finales en émission Duos/Groupe
	- +Corrigé: 2 ou 3 erreurs de type de manche
	- +Corrigé: Sélection du bon profil après déplacement des données

## Changelog de la Version Officielle
  - 1.135
    - Bug fixes and multilanguage updates
  - 1.134
    - Many updates from the community. Multilanguage / Profile Editing / Various Fixes
  - 1.133
    - Add Bean Hill Zone and fix names (Thanks to iku55 & Foolyfish)
  - 1.132
    - Season 9 (aka Season 3)
  - 1.131
    - Update by ThreesFG to fix date parsing
  - 1.130
    - Update by ThreesFG to fix log parsing for new update
  - 1.129
    - Try and fix Leading Light
  - 1.128
    - Update Season filter
  - 1.127
    - Season 8 (aka Season 2)
  - 1.126
    - Fix some log parsing
  - 1.125
    - Move Blast Ball to final category
  - 1.124
    - Season 7 (aka Season 1)
  - 1.123
    - Fix group play stats
  - 1.122
    - Add Sweet Thieves
  - 1.121
    - Possibly fix Sum Fruit
  - 1.120
    - Season 6 update
  - 1.119
    - Round name fix
  - 1.118
    - Season 5 update
  - 1.117
    - Season 4.5 update
  - 1.116
    - Fix for round names
  - 1.115
    - Season 4 update
  - 1.114
    - Fix overlay not showing correct stats for Snowball Survivor
  - 1.113
    - Fix Snowball Survival
  - 1.112
    - Update for new game patch
  - 1.111
    - Try and fix false negatives by removing NDI
  - 1.110
    - Fix error reading log file date/time in rare instances
  - 1.109
    - Add ability to track if a final is actually a final
  - 1.108
    - Fix Hex a gone game mode from showing all wins/finals
    - Fix font selector not remembering font
  - 1.107
    - Add Font Chooser for overlay
    - Fix Average times on main grid
  - 1.106
    - Added better options for the cycle stats on overlay in settings
    - Added average finish time to main grid
    - Minor sorting fixes
  - 1.105
    - Grid sorting improvements
    - Display issue with private lobby stats on overlay
  - 1.104
    - Implemented a number of improvements from hunterpankey
  - 1.103
    - Fix log reading during live rounds
  - 1.102
    - Fix issues reading log file in certain cases
    - Made sure private lobbies stats dont show in main screen
  - 1.101
    - Add ability to track private lobbies
  - 1.100
    - Fixes and added levels for Season 3
  - 1.99
    - Hopefully made it so game modes wont affect levels anymore
  - 1.98
    - Logic to handle new game mode
    - Added ability to only show certain stats on overlay instead of having to cycle them
  - 1.97
    - Logic to handle new game mode
  - 1.96
    - Fixed existing levels for the northernlion game mode to show up correctly
  - 1.95
    - Fixed new game mode adding levels that shouldn't be there
  - 1.94
    - Fixed typo in level name
  - 1.93
    - Added ability to rename Hoopsie Legends to Hoopsie Heroes
    - Added logic to save main window size
  - 1.92
    - Added code to handle levels with variations in their name
  - 1.91
    - Added Big Fans Level
  - 1.90
    - Fixed names on overlay
  - 1.89
    - Fixed names for new Slime Event levels
  - 1.88
    - Added more info to AssemblyInfo to possibly help with false positives in AV programs
  - 1.87
    - Fixed level names in level details for gauntlet matches
    - Allowed main window to be resizable
  - 1.86
    - Fixed Level stats grid columns
  - 1.85
    - Finish time on overlay will now become gold when you beat overall best time or green when you beat best time for current filter
    - Time on overlay will now also show the timeout duration
  - 1.84
    - Fixed a filter issue with profiles
  - 1.83
    - Added ability to switch between a Main and Practice profile
  - 1.82
    - Fixed season filter dates
  - 1.81
    - Fixed guantlet levels not showing up on Overlay properly
  - 1.80
    - Added Final Streak to cycle with Win Streak
    - Added new maps
  - 1.79
    - Added option to cycle between Players and Server Ping on overlay
  - 1.78
    - Changed logic when not cycling stats on overlay to show the most interesting stat
    - Added option to show / hide percentages on overlay
  - 1.77
    - Added individual option for Cycle Qualify / Gold and Cycle Fastest / Longest to settings
  - 1.76
    - Moved Season 2 start date to Oct 8th
    - Added ability to choose when starting program to include previous stats or not
  - 1.75
    - Fixed streak count on overlay
  - 1.74
    - Fixed stat calculations for shows crossing filter boundries
    - Added some extra stats to the Wins Per Day popup
    - Added option in settings to show / hide Wins info for overlay
  - 1.73
    - Added options to settings screen for overlay color and flip to make it more visible to the user
    - Added ability to manually resize overlay from the corners
  - 1.72
    - Changed overlay so it stays visible when you minimize amin screen
  - 1.71
    - Changed main screen to show Fastest / Longest qualifications for each level
    - Fixed minor sorting issue in the grids
  - 1.70
    - Cleaned up auto update feature a bit
  - 1.69
    - Program will save last location of main window now and restore it when opened again
  - 1.68
    - Fixed Week / Day filters
    - Added more filter options in settings
    - Added logic to account for new levels that may come up in Season 2
    - Added option to auto update program in settings
  - 1.67
    - Fixed times in database to be stored correctly as utc
  - 1.66
    - Hopefully fixed an issue with times counting down on overlay in rare cases
  - 1.65
    - Added export to MarkDown option
    - Added ability to click on Win% label to toggle columns to show %s and back
  - 1.64
    - Fixed time issue when parsing log
  - 1.63
    - Added export options for both Html and BBCode when right clicking any grid
  - 1.62
    - Fixed some logic when deleting shows while filtered
    - Switched the Longest/Fastest to align with Qualify/Gold
  - 1.61
    - Added logic to reset overlay position if it ended up off screen
    - Tightened up the overlay when hiding the round info
  - 1.60
    - Added option to show tab on overlay for current filter
  - 1.59
    - Try and make sure deleted shows dont get added back accidentally
  - 1.58
    - Fixed rare case when deleting show didnt work
  - 1.57
    - Fixed overlay missing image on startup
  - 1.56
    - Add ability to show / hide information on overlay
  - 1.55
    - Fixed overlay getting out of wack if you change filters a lot
  - 1.54
    - Added mouse hover tool tip on Qualified / Gold / Silver / Bronze to show value as a %
  - 1.53
    - Fixed Filters on overlay not taking into account UTC time
  - 1.52
    - Fixed Time display on overlay not updating sometimes
  - 1.51
    - Fixed an issue around results coming from a previous match
  - 1.50
    - Fixed accidental debug typo
  - 1.49
    - Added filter options to settings page for overlay display
  - 1.48
    - Fixed Gold statistic on overlay, was using wrong medal type
  - 1.47
    - Added Gold statistic to overlay that rotates with Qualify
  - 1.46
    - Fixed overlay display not updating
  - 1.45
    - Cleaned up labels on overlay
  - 1.44
    - Fixed end of round numbers on overlay
  - 1.43
    - Added ability to delete Shows in Shows screen