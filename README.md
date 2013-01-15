SimoBot
=======

(Significant) Features:

### Markov chains
When highlighted with 1-2 words, uses them as source and responds to the highlighter with a short sentence made with markov chains from (Simobot just fetches the stuff from redis. The actual source text is loaded to redis outside Simobot, although it does add new lines on the channel in real time) channel logs, the bible, etc. If no words are given as a source, uses a random key in redis as the source. Funny stuff

### Last.fm API usage
On !np fetches the !np-sayers nick from a saved list (added to with !setlastfm <last.fm nick>) and gives a now playing thingy plus a few tags for the song added by people at last.fm (the stuff inside parentheses). It does this using Last.fm API, f.ex: 

> 21:51:03 <@Tsarpf> !np

> 21:51:05 < SimoBot> Tsarpf playing: \<BLOKHE4D & Receptor> - \<Bass Dust> (Drum and bass, techstep, liquid funk)

### !expl and !add.

!add adds a new expl. same word can have multiple expls which are separated with a '|' ...
> !add \<explanation_name> \<some sort of free form explanation for the word or something>


!expl <word> gets the saved explanation, or if no word given, gets a random one.

Expl is at it's best when it's used to explain and save in-jokes the channel has etc.

### Wikipedia
!wiki <article name> gets the first two sentences from a wikipedia article using the wikipedia API.
