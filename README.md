# Sommaire

  - [Tracker Fall Guys "FE" (Frenchy Edition) par Micdu70](#tracker-fall-guys-fe-frenchy-edition-par-micdu70)
    - [Téléchargement](#téléchargement)
    - [Utilisation](#utilisation)
    - [FAQ](#faq)
    - [Thème](#thème)
      - [Thème Clair](#thème-clair)
      - [Thème Sombre](#thème-sombre)
  - [Langues disponibles](#langues-disponibles)
  - [Overlay](#overlay)
    - [Raccourcis](#raccourcis)
    - [Créer vos propres arrière-plans](#créer-vos-propres-arrière-plans)
  - [Profil](#profil)
    - [Lier un profil à une émission spécifique](#lier-un-profil-à-une-émission-spécifique)
    - [Supprimer des émissions ou déplacer des émissions vers un autre profil](#supprimer-des-émissions-ou-déplacer-des-émissions-vers-un-autre-profil)
  - [Changelog complet de la "FE" (Frenchy Edition)](#changelog-complet-de-la-fe-frenchy-edition)
  - [Changelog récent de la Version Officielle](#changelog-récent-de-la-version-officielle)


# Tracker Fall Guys "FE" (Frenchy Edition) par Micdu70

Programme qui permet de récupérer les stats de Fall Guys (via la lecture des logs) pour suivre ce que vous faites en jeu.

INFO: La Version Officielle est disponible ici => https://github.com/ShootMe/FallGuysStats


## Téléchargement

**Dernière version: 1.163** *~ 28/04/2023*

　　<a href="https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/FallGuysStats.zip">![FallGuysStats.zip](Resources/FallGuysStats-download.svg)</a>
  - Si votre logiciel antivirus bloque l'utilisation du tracker, utilisez alors la version ci-dessous qui ne possède pas la fonction de MAJ automatique.

　　<a href="https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/FallGuysStatsManualUpdate.zip">![FallGuysStats.zip](Resources/FallGuysStatsManualUpdate-download.svg)</a>


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


### Fenêtre Principale

![Fenêtre Principale](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/mainWindow.png)


### Liste des Stats des Manches

![Stats des Manches](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/levelWindow.png)


## FAQ

**Q.1) Que signifie une ligne de couleur grise (thème clair) ou noire (thème sombre) dans la liste des stats des manches ?**

------> Cela peut signifier deux choses : Soit une manche jouée dans une partie personnalisée ou soit une manche jouée dans une émission qui a été quittée avant la fin de celle-ci.


## Thème

  - Le tracker supporte actuellement deux thèmes: "Clair" et "Sombre".


### Thème Clair

![Thème Clair - Fenêtre Principale)](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/mainWindowLightTheme.png)

![Thème Clair - Stats des Manches)](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/levelWindowLightTheme.png)


### Thème Sombre

![Thème Sombre - Fenêtre Principale](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/mainWindowDarkTheme.png)

![Thème Sombre - Stats des Manches](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/levelWindowDarkTheme.png)


## Langues disponibles

  - Le Tracker Fall Guys "FE" supporte les langues suivantes:
    - 🇺🇸 English (Anglais)
    - 🇫🇷 Français
    - 🇰🇷 한국어 (Coréen)
    - 🇯🇵 日本語 (Japonais)
    - 🇨🇳 简体中文 (Chinois Simplifié)


## Overlay

![Overlay](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/overlay.png)


### Raccourcis

  - Cliquer une fois sur l'overlay pour le sélectionner puis:
    - Appuyer sur **'Ctrl + Maj + D'** pour remettre les dimensions par défaut de l'overlay.
    - Appuyer sur la touche **'T'** pour changer la couleur de l'arrière-plan.
    - Appuyer sur la touche **'F'** pour inverser horizontalement l'affichage.
    - Appuyer sur la touche **'P'** pour passer au profil suivant.
    - Appuyer sur les touches des **chiffres situés au dessus des lettres (1 à 9)** pour changer de profil.
    - Maintenir la touche **'Maj' enfoncée et utiliser la molette de votre souris** pour changer de profil.
    - Maintenir la touche **'Maj' enfoncée et utiliser la touche directionnelle 'Droite'/'Bas' ou 'Gauche'/'Haut'** pour changer de profil.
    - Appuyer sur la touche **'C'** pour afficher le nombre de joueurs par support de jeu.
    - Appuyer sur la touche **'R'** pour colorer le nom des manches selon leur type.


### Créer vos propres arrière-plans

![Overlay Modifié](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/customizedOverlay.png)

  - **Prérequis** Lancer au moins une fois le tracker Fall Guys pour obtenir un dossier nommé "Overlay".


  - **Étape 1.** Modifier l'image **background.png** et **tab.png** qui sont dans le dossier "Overlay" (où se trouve le fichier d'exécution du tracker).


  - **Étape 2.** Nommer les images modifiées comme ci-dessous.
    - **{Nom de mon image}** doit être le même pour les deux fichiers
      - background_**{Nom de mon image}**.png
      - tab_**{Nom de mon image}**.png


  - **Étape 3.** Si pas déjà fait, placer les deux images dans le dossier "Overlay" (où se trouve le fichier d'exécution du tracker).


  - **Étape 4.** Vous verrez maintenant dans le menu "Configuration" du tracker vos propres arrière-plans en haut de la liste "Arrière-plan (image)" pour l'overlay.


  - **Étape 5.** Sélectionner l'arrière-plan souhaité puis 'Enregistrer' pour l'appliquer.


## Profil

### Lier un profil à une émission spécifique

  - Permet de changer de profil automatiquement au moment où l'émission spécifiée commence.

  - Menu "Profil" > "Config. des profils"

![Configuration des profils](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/profileAndShowLinkage.png)

  - Menu "Configuration" -> Option "Passer automatiquement sur le profil lié"

![Configuration - Profil lié](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/automaticProfileChange.png)


### Supprimer des émissions ou déplacer des émissions vers un autre profil

![Émissions](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/showsWindow.png)

  - Cliquer sur "Émissions: X" en haut de la fenêtre principale pour voir les stats des émissions.
  - Sélectionner une ou plusieurs émissions avec la touche **'Ctrl'** ou avec la combinaison de touches **'Ctrl + Maj'**.
  - Faire un clique-droit sur la sélection pour pouvoir déplacer ou supprimer celle-ci.


## Bug(s) connu(s) dans la dernière version de la "FE" (Frenchy Edition)

  - Tous les bugs connus ont normalement été résolus dans la v1.161 !


## Changelog complet de la "FE" (Frenchy Edition)

  - 1.161/1.162/1.163 *~ 28/04/2023*
  { Correction de bugs de la "FE" }
    - +Ajouté: Option (activée par défaut) pour envoyer l'info des manches jouées vers le site <a href="https://fallalytics.com/">Fallalytics</a> (merci à @Hoier)
    - +Ajouté: Overlay => Affichage du nombre d'haricots qui ont réussi la manche (fini la course/survécu)

  - 1.160 *~ 21/04/2023*
    - +Corrigé: Overlay => Divers petites corrections ["FE"]
    - +Corrigé: Tri des manches par ordre alphabétique dans la fenêtre principale pour toutes les langues [Version Officielle]

  - 1.159 *~ 19/04/2023*
    - +Corrigé: *HOTFIX* Infos du graphique du nombre de victoires par jour

  - 1.158 *~ 18/04/2023*
    - +Corrigé: Détection de la manche finale pour l'émission "Fol'Virevoltes Tropicales"
    - +Corrigé: Infos du graphique du nombre de victoires par jour
    - ++Changé: Aucun temps "Finish" ne sera enregistré quand vous serez "hors-jeu pour cette manche" mais que votre équipe arrive tout de même à se qualifier
    - ++Changé: Overlay => Divers petits changements concernant la récupération des infos ["FE"]

  - 1.157 *~ 09/04/2023*
  { Cette version, bien que basée sur la Version Officielle (v1.146) datant du 09/04/2023, n'applique pas - par choix personnel - toutes les modifications de celle-ci }
    - +Corrigé: Le premier champ de texte (celui le plus haut) ne sera plus sélectionnée dès l'ouverture de la fenêtre "Configuration" ["FE"]
    - ++Changé: Le temps "Finish" enregistré sera égal à la durée de la manche quand vous serez éliminé(e) mais que votre équipe arrive à se qualifier tout de même
    - ++Changé: Le graphique du nombre de victoires par jour a été amélioré [Version Officielle + Modifications "FE"]
    - ++Ajouté: Arrière-plan "Super Mario Bros. 3" pour l'overlay [Version Officielle]

  - 1.156 *~ 07/04/2023*
    - +Corrigé: Détection comme manche finale pour les émissions de type "Contre-la-montre"
    - +Corrigé: Bug (uniquement visuel) des valeurs dans la fenêtre principale du tracker dans certains scénarios
    - ++Changé: Les émissions non terminées seront maintenant traitées comme si c'était des Parties Personnalisées => Pour une meilleure visibilité entre les émissions terminées et non terminées
    - ++Changé: Divers changements mineurs...

  - 1.155 *~ 05/04/2023*
  { Correction de bugs de la "FE" }

  - 1.154 *~ 04/04/2023*
  { Cette version est basée sur la Version Officielle (v1.143) datant du 01/04/2023 => https://github.com/ShootMe/FallGuysStats }
  { Correction de bugs de la "FE" }
    - ++Changé: Les stats des émissions non terminés sont maintenant aussi enregistrées dans le tracker !
    - ++Changé: Le graphique du nombre de victoires par jour a été amélioré
    - ++Changé: Les options/réglages "par défaut" sont:
      1) Thème "Sombre" sélectionnée
      2) "MAJ. auto du tracker" sélectionnée
      3) L'overlay est affiché
      4) "Masquer les pourcentages" pour l'overlay sélectionnée
      5) "Joueurs seul." pour l'overlay sélectionnée (au lieu de "Cycle \*Joueurs / Ping\*")
      6) "Filtre 'Stats' et 'Parties'" sélectionnée pour les filtres "\*Wins / Finales\*" et "\*Qualif. / Or\*" sur l'overlay
      7) "Toutes les stats" sélectionnée pour le filtre "\*Meilleur / Plus long\*" sur l'overlay
      8) "Afficher les joueurs par support de jeu" pour l'overlay sélectionnée
      9) "Colorer la manche selon son type" pour l'overlay sélectionnée
     10) "Passer automatiquement sur le profil lié" pour l'overlay **N'EST PAS** sélectionnée
	 11) Le graphique du nombre de victoires est en forme de barres (au lieu de points)

  - 1.152 et 1.153 *~ 31/03/2023*
  { Correction de bugs de la "FE" }
    - ++Changé: Le graphique du nombre de victoires par jour a été amélioré
    - ++Changé: Les options/réglages "par défaut" sont:
      1) Thème "Sombre" sélectionnée
      2) "MAJ. auto du tracker" sélectionnée
      3) L'overlay est affiché
      4) "Masquer les pourcentages" pour l'overlay sélectionnée
      5) "Joueurs seul." pour l'overlay sélectionnée (au lieu de "Cycle \*Joueurs / Ping\*")
      6) "Filtre 'Stats' et 'Parties'" sélectionnée pour les filtres "\*Wins / Finales\*" et "\*Qualif. / Or\*" sur l'overlay
      7) "Toutes les stats" sélectionnée pour le filtre "\*Meilleur / Plus long\*" sur l'overlay
      8) "Afficher les joueurs par support de jeu" pour l'overlay sélectionnée
      9) "Colorer la manche selon son type" pour l'overlay sélectionnée
     10) "Passer automatiquement sur le profil lié" pour l'overlay **N'EST PAS** sélectionnée

  - 1.151 *~ 30/03/2023*
  { Cette version est basée sur la Version Officielle (v1.142) datant du 30/03/2023 => https://github.com/ShootMe/FallGuysStats }
    - +Rappel+: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
	- ++Changé: Tout plein de chose à découvrir !

  - 1.150 *~ 20/03/2023*
    - +Rappel+: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - +Corrigé: Les fitres 'en Solo' et 'en Groupe' dans le menu "Filtres => Parties" de la fenêtre principale ont été corrigés **ATTENTION: Toutes les émissions enregistrées avant l'utilisation de cette version ne seront pas filtrées correctement !**
    - +Corrigé: Le "bon" logo est maintenant visible en haut à gauche de toutes les fenêtres du programme

  - 1.149 *~ 20/03/2023*
    - +Rappel+: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - ++Changé: Réduction de la hauteur du menu de la liste des arrière-plans pour l'overlay afin d'empêcher le défilement automatique vers le bas selon la position du curseur
	- +Corrigé: Visuel de l'interface utilisateur dans la fenêtre de configuration

  - 1.148 *~ 19/03/2023*
    - +Rappel+: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - +Corrigé: L'option "MAJ auto. du tracker" n'était plus visible dans la version précédente
    - ++Ajouté: Overlay => Arrière-plan "Aliens"
    - ++Ajouté: Overlay => Arrière-plan "Pop-corn"
    - ++Changé: Les options/réglages "par défaut" sont:
      1) Langue 'Français' sélectionnée
      2) "MAJ. auto du tracker" sélectionnée
      3) L'overlay est affiché
      4) "Masquer les pourcentages" pour l'overlay sélectionnée
      5) "Joueurs seul." pour l'overlay sélectionnée (au lieu de "Cycle \*Joueurs / Ping\*")
      6) "Filtre 'Stats' et 'Parties'" sélectionnée pour les filtres "\*Wins / Finales\*" et "\*Qualif. / Or\*" sur l'overlay
      7) "Toutes les stats" sélectionnée pour le filtre "\*Meilleur / Plus long\*" sur l'overlay
      8) "Afficher joueurs par support de jeu" pour l'overlay sélectionnée
      9) "Colorer manches selon leur type" pour l'overlay sélectionnée
     10) "Passer auto. sur le profil lié" pour l'overlay sélectionnée

  - 1.147 *~ 18/03/2023*
  { Cette version possède les dernières modifications faites par @qutrits => https://github.com/qutrits/FallGuysStats }
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - Pleins de changement à découvrir ! :)

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
    - ++Ajouté: Bouton pour verrouiller/déverrouiller la position de l'overlay (merci à @qutrits)
    - ++Changé: Le tableau des stats des victoires a été amélioré (merci à @qutrits)
    - ++Changé: Nouveau système de MAJ automatique du programme (via utilisation d'un fichier .bat)
    - ++Changé: Divers changements (merci à @qutrits)

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

  - 1.146
    - Bug fixes
  - 1.145
    - Fixed additional prevent mouse cursor bugs **[Correction pas appliquée dans la "FE"]**
  - 1.144
    - Change Settings UI **[Changement pas appliqué dans la "FE"]**
    - Add prevent mouse cursor bugs (Experimental) **[Option enlevée dans la "FE"]**
  - 1.143
    - Add style option to the daily win stats graph
  - 1.142
    - Add overlay background opacity adjustment option
  - 1.141
    - Upgrade Win Per Day Charts
    - Finals bug fix
  - 1.140
    - Update all packages due to package vulnerabilities
  - 1.139
    - Bug fixes and Correct typos
  - 1.138
    - Added theme option and change overlay background image option
  - 1.137
    - Added a function to automatically select a linked profile when a show starts by linking a profile with a show
  - 1.136
    - Overlay position fixed function button addition and changed the graph to make it look better
  - 1.135
    - Bug fixes and multilanguage updates
  - 1.134
    - Many updates from the community. Multilanguage / Profile Editing / Various Fixes
  - 1.133
    - Add Bean Hill Zone and fix names (Thanks to iku55 & Foolyfish)
  - 1.132
    - Season 9 (aka Season 3)
  - [...]
