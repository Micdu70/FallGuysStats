# Fall Guys Stats "FE" (Frenchy Edition) par Micdu70
Programme qui permet de récupérer les stats de Fall Guys (via la lecture des logs) pour suivre ce que vous faites en jeu.

INFO: La Version Officielle est disponible ici => https://github.com/ShootMe/FallGuysStats

## Téléchargement
**Dernière version: 1.147** *~ 16/03/2023*

  - => [FallGuysStats.zip](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/FallGuysStats.zip)

***-OU-***

  - Si votre logiciel antivirus bloque l'utilisation du tracker, utilisez alors cette version qui ne possède pas la fonction de MAJ automatique => [FallGuysStatsManualUpdate.zip](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/FallGuysStatsManualUpdate.zip)
  
## Utilisation
  - Première fois:
    - Extraire le contenu du fichier zip téléchargé vers un nouveau dossier vide
    - Dans ce nouveau dossier, lancer le tracker
    - Configurer le tracker comme bon vous semble (afficher l'overlay/modifier des options/etc.)
    - Lancer Fall Guys

  - Mise à jour (depuis une Version Officielle ou d'une ancienne version "FE"):
    - Extraire le contenu du fichier zip téléchargé vers le dossier contenant l'ancienne version du tracker à remplacer
	- Lancer la nouvelle version du tracker
    - Configurer le tracker comme bon vous semble (afficher l'overlay/modifier des options/etc.)
    - Lancer Fall Guys

 **IMPORTANT: Les stats des émissions seront enregistrées dans le tracker UNIQUEMENT à la fin de celles-ci.**

![Fall Guys Stats "FE"](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/mainWindow.png)

![Stats des manches de Fall Guys](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/levelWindow.png)

## Langues disponibles
  - Fall Guys Stats "FE" supporte les langues suivantes :
    - 🇺🇸 English (Anglais)
    - 🇫🇷 Français *[ langue par défaut ]*
    - 🇰🇷 한국어 (Coréen)
    - 🇯🇵 日本語 (Japonais)
    - 🇨🇳 简体中文 (Chinois Simplifié)

## Overlay
![Overlay](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/overlay.png)

  - Appuyez sur la touche 'T' pour changer la couleur de l'arrière-plan.
  - Appuyez sur la touche 'F' pour inverser horizontalement l'affichage.
  - Appuyez sur touche 'P' pour passer au profil suivant.
  - Appuyez sur les touches des chiffres situés au dessus des lettres (1 à 9) pour changer de profil.
  - Maintenez la touche 'Maj' enfoncée et utilisez la molette de votre souris pour changer de profil.
  - Maintenez la touche 'Maj' enfoncée et utilisez la touche directionnelle 'Droite'/'Bas' ou 'Gauche'/'Haut' pour changer de profil.
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
    *=> Partiellement résolu depuis la version 1.141 (via suppression du numéro de la manche)*
	
  6) L'info "Temps" de l'overlay peut se mettre à "clignoter" par moment (souvent entre deux manches)
    *=> Pas de solution trouvée pour l'instant*

## Changelog complet de la "FE" (Frenchy Edition)
  - 1.147 *~ 16/03/2023*
  { Cette version possède les dernières modifications faites par @qutrits => https://github.com/qutrits/FallGuysStats }
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
	- Added overlay background image change option
	- Added a function to automatically select a linked profile when a show starts by linking a profile with a show
	- Added rounds logo
  - 1.146 *~ 15/03/2023*
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
	- ++Changé: Le titre de la fenêtre principale du tracker peut être traduit dans toutes les langues ('Français' et 'English' ont été fait)
	- ++Changé: Changements mineurs pour l'overlay (sur certaines manches)
  - 1.145 *~ 09/03/2023*
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
	- +Corrigé: L'ordre et le tri des manches, dans la fenêtre principale du tracker, pour la langue 'Français' et 'English'
  - 1.144 *~ 07/03/2023*
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
	- ++Changé: Les deux manches "Survie" avec un hoverboard sont maintenant configurées comme si c'était des manches "Course" => L'overlay se basera sur le meilleur temps réalisé - au lieu du plus long temps - pour colorer l'info 'Finish' (en cas de record personnel par exemple)
	- ++Changé: L'ordre initial des manches, dans la fenêtre principale du tracker, sera en fonction de leur nom en français (au lieu de l'anglais)
  - 1.143 *~ 06/03/2023*
    - {-Hotfix-} Correction de l'affichage du bouton pour verrouiller/déverrouiller la position de l'overlay quand l'option "Afficher les onglets d'info du filtre et profil actuels" est activée
  - 1.142 *~ 06/03/2023*
  { Cette version possède les dernières modifications faites par @qutrits => https://github.com/qutrits/FallGuysStats }
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
	- ++Ajouté: Bouton pour verrouiller/déverrouiller la position de l'overlay (par @qutrits)
	- ++Changé: Le tableau des stats des victoires a été amélioré (par @qutrits)
    - ++Changé: Nouveau système de MAJ automatique du programme (via utilisation d'un fichier .bat)
	- ++Changé: Divers changements (par @qutrits et moi-même)
  - 1.141 *~ 04/03/2023*
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
	- ++Changé: Pour les manches non jouées (mode spectateur), l'overlay n'affichera aucun numéro de manche
	- +Corrigé: Détection de la finale "Ascension Gélatineuse" dans l'émission "Trek Gélatineux"
  - 1.140 *~ 03/03/2023*
    - ++Ajouté: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - ++Ajouté: Overlay => L'information "Finish" affiche maintenant votre position lorsque vous êtes éliminé(e)
    - ++Changé: Overlay => L'information "Finish" passe maintenant en rose lorsque vous êtes éliminé(e)
    - ++Changé: Votre position indiquera maintenant toujours "1er" pour la dernière manche d'une émission gagnée
    - ++Changé: Divers petits changements...
  - 1.139 *~ 26/02/2023*
    - ++Ajouté: Fenêtre de sélection de la langue au tout premier démarrage du tracker
	- +Corrigé: Type de manche affiché dans l'overlay et dans la liste des stats des manches, concernant le Volleyfall, dans les émissions "Duos" et "Spéciale Groupe"
	- ++Changé: Divers changements mineurs...
  - 1.138 *~ 22/02/2023*
    - ++Hotfix: Votre ancienne configuration ne sera pas perdue si vous venez de la Version Officielle du tracker\*
	- \* Cependant, les options "par défaut" données ci-après seront appliquées au passage à la "FE"
    - ++Changé: Les options "par défaut" sont:
	1) Langue 'Français' sélectionnée
	2) "MAJ. auto du tracker" activée
	3) "Joueurs seul." pour l'overlay sélectionnée (au lieu de "Cycle \*Joueurs / Ping\*")
	4) "Afficher joueurs par type de support" pour l'overlay activée
	5) "Colorer manches selon leur type" pour l'overlay activée

  - 1.137 *~ 21/02/2023*
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
