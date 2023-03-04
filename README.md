# Fall Guys Stats "FE" (Frenchy Edition) par Micdu70
Programme qui permet de récupérer les stats de Fall Guys (via la lecture des logs) pour suivre ce que vous faites en jeu.

INFO: La Version Officielle est disponible ici => https://github.com/ShootMe/FallGuysStats

## Téléchargement
**Dernière version: 1.140**

  - => [FallGuysStats.zip](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/FallGuysStats.zip)

***-OU-***

  - Si votre logiciel antivirus bloque l'utilisation du programme, utilisez alors cette version qui ne possède pas la fonction de MAJ automatique => [FallGuysStatsManualUpdate.zip](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/FallGuysStatsManualUpdate.zip)
  
## Utilisation
  - Extraire le contenu du fichier zip téléchargé vers un nouveau dossier vide
  - Dans ce nouveau dossier, lancer le programme
  - Configurer le programme comme bon vous semble (afficher l'overlay/modifier des options/etc.)
  - Lancer Fall Guys
 
 **IMPORTANT: Les stats des émissions seront enregistrées dans le programme UNIQUEMENT à la fin de celles-ci.**

![Fall Guys Stats "FE"](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/mainWindow.png)

![Stats des manches de Fall Guys](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/levelWindow.png)

## Langues disponibles
  - Fall Guys Stats "FE" supporte les langues suivantes :
    - English (Anglais)
    - Français *[ langue par défaut ]*
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
  - Appuyez sur la touche 'C' pour afficher le nombre de joueurs par support de jeu.
  - Appuyez sur la touche 'R' pour colorer le nom des manches selon leur type.

## Supprimer des émissions ou déplacer des émissions vers un autre profil
![Supprimer une ou plusieurs émissions](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/showsWindow.png)

  - Cliquer pour voir les stats des émissions.
  - Sélectionner une ou plusieurs émissions avec la touche 'Ctrl' ou avec la combinaison de touches 'Ctrl'+'Maj'.
  - Faire un clique-droit sur la sélection pour pouvoir déplacer ou supprimer celle-ci.

## Bug(s) connu(s) de la dernière version "FE" (Frenchy Edition)
  1) Si des stats précédentes d'émissions sont détectées, certaines manches où vous êtiez éliminé(e) peuvent avoir un temps "Finish"
    *=> Pour éviter ce bug, ne pas oublier de lancer le tracker avant de jouer au jeu/lancer une partie*

  2) L'info "Temps" sur l'overlay ne s'arrête pas en émission solo sur des manches de type survie - ou manches avec gélatine - en cas d'élimination
    *=> Le temps s'arrêtera au chargement de la prochaine manche ou au lancement d'une recherche d'une autre partie*
	
  3) L'info "Temps" sur l'overlay commence avant le compte-à-rebours de début de manche
    *=> Survient après le bug 2). Cependant, le temps redémarre correctement à la fin du compte-à-rebours de début de manche*
	
  4) L'info "Temps" et "Finish" de l'overlay ne s'arrêtent pas sur une manche non jouée (mode spectateur) si vous quittez la partie
    *=> Ces infos seront stoppées au chargement de la première manche de la prochaine partie*
	
  5) Le numéro de la manche dans l'overlay est erroné pour les manches non jouées (mode spectateur)
    *=> Partiellement résolu dans la version 1.141 (via suppression du numéro de la manche)*
	
  6) L'info "Temps" de l'overlay peut se mettre à "clignoter" par moment (entre deux manches)
    *=> Pas de solution trouvée pour l'instant*

## Changelog complet de la "FE" (Frenchy Edition)
  - 1.141
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
	- ++Changé: Pour les manches non jouées (mode spectateur), l'overlay n'affichera pas de numéro de manche maintenant
	- +Corrigé: Détection de la finale "Ascension Gélatineuse" dans l'émission "Trek Gélatineux"
  - 1.140
    - ++Ajouté: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - ++Ajouté: Overlay => L'information "Finish" affiche maintenant votre position lorsque vous êtes éliminé(e)
    - ++Changé: Overlay => L'information "Finish" passe maintenant en rose lorsque vous êtes éliminé(e)
    - ++Changé: Votre position indiquera maintenant toujours "1er" pour la dernière manche d'une émission gagnée
    - ++Changé: Divers petits changements...
  - 1.139
    - ++Ajouté: Fenêtre de sélection de la langue au tout premier démarrage du programme
	- +Corrigé: Type de manche affiché dans l'overlay et dans la liste des stats des manches, concernant le Volleyfall, dans les émissions "Duos" et "Spéciale Groupe"
	- ++Changé: Divers changements mineurs...
  - 1.138
    - ++Hotfix: Votre ancienne configuration ne sera pas perdue si vous venez de la Version Officielle du tracker\*
	- \* Cependant, les options "par défaut" données ci-après seront appliquées au passage à la "FE"
    - ++Changé: Les options "par défaut" sont:
	1) Langue 'Français' sélectionnée
	2) "MAJ. auto du tracker" activée
	3) "Joueurs seul." pour l'overlay sélectionnée (au lieu de "Cycle \*Joueurs / Ping\*")
	4) "Afficher joueurs par type de support" pour l'overlay activée
	5) "Colorer manches selon leur type" pour l'overlay activée

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

## Changelog récent de la Version Officielle
  - 1.135
    - Bug fixes and multilanguage updates
  - 1.134
    - Many updates from the community. Multilanguage / Profile Editing / Various Fixes
  - 1.133
    - Add Bean Hill Zone and fix names (Thanks to iku55 & Foolyfish)
  - 1.132
    - Season 9 (aka Season 3)
  - [...]
