using System.Collections.Generic;
using UnityEngine;

public class ValuableLovePotion : MonoBehaviour
{
	private enum State
	{
		Idle = 0,
		Active = 1
	}

	private PhysGrabObject physGrabObject;

	private List<string> transitiveVerbs = new List<string>();

	private List<string> intransitiveVerbs = new List<string>();

	private List<string> adjectives = new List<string>();

	private List<string> intensifiers = new List<string>();

	private List<string> nouns = new List<string>();

	private List<string> adverbs = new List<string>();

	private float coolDownUntilNextSentence = 3f;

	private ParticleSystem particles;

	private bool particlesPlaying;

	public Renderer LovePotionRenderer;

	private State currentState;

	private string playerName = "[playerName]";

	private void Start()
	{
		particles = GetComponentInChildren<ParticleSystem>();
		physGrabObject = GetComponent<PhysGrabObject>();
		InitializeWordLists();
	}

	private void Update()
	{
		LovePotionRenderer.material.mainTextureOffset = new Vector2(0f, Time.time * 0.1f);
		LovePotionRenderer.material.mainTextureScale = new Vector2(2f + Mathf.Sin(Time.time * 1f) * 0.25f, 2f + Mathf.Sin(Time.time * 1f) * 0.25f);
		if (physGrabObject.grabbed)
		{
			if (!particlesPlaying)
			{
				particles.Play();
				particlesPlaying = true;
			}
		}
		else if (particlesPlaying)
		{
			particles.Stop();
			particlesPlaying = false;
		}
		if (SemiFunc.IsMultiplayer())
		{
			switch (currentState)
			{
			case State.Idle:
				StateIdle();
				break;
			case State.Active:
				StateActive();
				break;
			}
		}
	}

	private void StateIdle()
	{
		if (coolDownUntilNextSentence > 0f && physGrabObject.grabbed)
		{
			coolDownUntilNextSentence -= Time.deltaTime;
		}
		else
		{
			if (!PhysGrabber.instance || !PhysGrabber.instance.grabbed || !PhysGrabber.instance.grabbedPhysGrabObject || !(PhysGrabber.instance.grabbedPhysGrabObject == physGrabObject))
			{
				return;
			}
			bool flag = false;
			if (!SemiFunc.IsMultiplayer())
			{
				playerName = "this potion";
				flag = true;
			}
			else
			{
				List<PlayerAvatar> list = SemiFunc.PlayerGetAllPlayerAvatarWithinRange(10f, PhysGrabber.instance.transform.position);
				PlayerAvatar playerAvatar = null;
				float num = float.MaxValue;
				foreach (PlayerAvatar item in list)
				{
					if (!(item == PlayerAvatar.instance))
					{
						float num2 = Vector3.Distance(PhysGrabber.instance.transform.position, item.transform.position);
						if (num2 < num)
						{
							num = num2;
							playerAvatar = item;
						}
					}
				}
				flag = true;
				if (playerAvatar != null)
				{
					playerName = playerAvatar.playerName;
				}
				else
				{
					playerName = "this potion";
				}
			}
			if (flag)
			{
				string message = GenerateAffectionateSentence();
				currentState = State.Active;
				Color possessColor = new Color(1f, 0.3f, 0.6f, 1f);
				ChatManager.instance.PossessChatScheduleStart(10);
				ChatManager.instance.PossessChat(ChatManager.PossessChatID.LovePotion, message, 1f, possessColor);
				ChatManager.instance.PossessChatScheduleEnd();
			}
		}
	}

	private void StateActive()
	{
		if (PhysGrabber.instance.grabbed && (bool)PhysGrabber.instance.grabbedPhysGrabObject && PhysGrabber.instance.grabbedPhysGrabObject != physGrabObject)
		{
			currentState = State.Idle;
			coolDownUntilNextSentence = Random.Range(5f, 15f);
		}
		else if (!ChatManager.instance.StateIsPossessed())
		{
			currentState = State.Idle;
			coolDownUntilNextSentence = Random.Range(5f, 15f);
		}
	}

	private void InitializeWordLists()
	{
		transitiveVerbs.AddRange(new string[136]
		{
			"adore", "sing oh la la with", "crush on", "fan over", "root for", "olala over", "geek out over", "vibe with", "fangirl over", "fanboy over",
			"heart", "olalalalala", "can't even", "obsess over", "trip over", "flip for", "freak over", "go nuts for", "go crazy for", "get hyped to",
			"hype up", "read a book with", "ride or die for", "show love for", "dance with", "ship", "low-key crush on", "have a thing for", "can't stop thinking about", "eyeing",
			"have robofeeling for", "catch crushie feelings for", "go heart eyes for", "get butterflies over", "have heart eyes for", "can't get over", "can't get enough of", "get in my feels over", "dream about", "imagine being happy with",
			"can't handle", "get weak for", "melt for", "have a soft spot for", "got a thing for", "obsessed with", "blushing over", "head over heels for", "feel the feels for", "admire",
			"crushing hard on", "can't even with", "totally into", "lost in", "robofeels about", "gaga for", "beeping and booping over", "extracting valuables with", "grooving to", "all about",
			"blown away by", "hyped about", "tripping over", "losing it over", "crying over", "obsessing over", "dying for", "looking at", "checking out", "having nothing but love for",
			"waiting on", "going wild for", "living for", "hooked on", "feeling", "hyped for", "showing mad love to", "sending hugs to", "sending hearts to", "0 1 1 0 1 1 0 0 0 1 1 0 1 1 1 1 0 1 1 1 0 1 1 0 0 1 1 0 0 1 0 1",
			"be friends with", "laugh hard with", "vibing with", "like", "cherish", "enjoy", "appreciate", "love", "treasure", "care for",
			"go gaga ding dong for", "long for", "think of", "miss", "want to be with", "smile at", "look at", "blush around", "get shy around", "laugh and cry with",
			"sing love songs with", "talk to", "listen to", "hold hands with", "share secrets with", "walk with", "sit with", "be near", "hang out with", "spend time with",
			"be around", "wink at", "wave to", "call", "write to", "sing to", "dance with", "cook for", "make art for", "make gifts for",
			"give gifts to", "surprise", "hug", "make taxman happy with", "ride the cart with", "robotickle", "do a stand-up routine for", "enjoy the sunset with", "share laughs with", "make smile",
			"bring joy to", "be silly with", "go on adventures with", "explore with", "go to Japan with", "grow old with"
		});
		intransitiveVerbs.AddRange(new string[140]
		{
			"vibe", "wow wow wow", "geek out", "daydream", "crush", "fangirl", "fanboy", "cheer", "freak out", "melt",
			"chill", "robostalk", "go oh la la", "robodance", "rock out", "hug", "kick it", "work", "match", "boot up",
			"meet up", "catch feels", "connect", "get along", "robohang", "check out", "look up", "catch up", "hang out", "tune in",
			"break", "tap in", "dive in", "get in", "be in", "collab", "share", "swap", "trade", "deal",
			"work", "play", "compete", "challenge", "engage", "join in", "get in on", "mix in", "add in", "help",
			"support", "follow", "track", "keep tabs", "keep up", "stay updated", "keep an eye", "buzz", "wow", "download the latest update",
			"update", "shout out", "yell", "scream", "hype", "beep boop beep boop", "vent", "express", "spill the beans", "confess love",
			"admit feelings", "declare bankruptcy", "dream", "blush", "imagine", "robodrool", "obsess", "admire", "go woop woop", "freak out",
			"lose it", "freak", "feel", "glow", "... i dunno ...", "trip", "groove", "beep boop", "crush", "flirt",
			"giggle", "smile", "laugh", "beam", "grin", "wonder", "wish", "hope", "long", "like",
			"leave a like and subscribe", "react", "lurk", "go OP", "despawn", "fart", "peep", "spy", "peek", "sigh",
			"breathe", "relate", "resonate", "go gaga ding dong", "boop", "jam", "flutter", "tingle", "twirl", "dance",
			"sing", "hum", "beep", "skip", "float", "sparkle", "bubble", "chirp", "glisten", "twinkle",
			"ponder", "admire", "breathe", "relax", "fancy", "laugh", "imagine", "melt", "smirk", "chuckle"
		});
		adjectives.AddRange(new string[137]
		{
			"epic", "awesome", "adorable", "adorbs", "fab", "cool", "dreamy", "snazzy", "rad", "stellar",
			"dope", "amazing", "breathtaking", "charming", "cute", "ah meh zin geh", "fresh", "funky", "glowing", "oh la la la",
			"incredible", "olala", "lovable", "lovely", "upgraded", "neat", "on point", "peachy", "0 1 1 0 0 0 1 1 0 1 1 1 0 1 0 1 0 1 1 1 0 1 0 0 0 1 1 0 0 1 0 1", "woop y woop y woo",
			"likey likey", "oooh ya ya", "slick", "smart", "smooth", "sparkling", "OMG", "stunning", "stylish", "ooooh yeaaa",
			"superb", "supreme", "sweet", "wowie", "trendy", "unreal", "vibrant", "wicked", "me likey", "beep boop",
			"oh my", "oof in a good way", "brilliant", "oh ya ya", "chic", "ah ... mazing", "clever", "comfy", "cu ... wait ... teh ... cute", "woop woop",
			"hugable", "cute", "yas queen", "sooo like ... wow", "divine", "electric", "elegant", "elite", "1337", "engaging",
			"enticing", "fancy", "fierce", "fire", "fly", "glam", "gorgeous", "hype mode", "iconic", "legendary",
			"litty", "wow wow wow", "magical", "majestic", "bootiiifoool", "poppin'", "precious", "C O O L ... cool", "C U T E ... cute", "... ... *blushes* ...",
			"slaying", "spicy", "robohandsome", "wowa wowa yas yas", "on fleek", "robocute", "wholesome", "winning", "robodorable", "powerful",
			"pretty", "beautiful", "sweet", "kind", "wow wow wow wow wow", "100%", "0 1 1 0 0 0 1 0 0 1 1 0 0 0 0 1 0 1 1 0 0 1 0 0", "fun", "brave", "interesting",
			"smart in the head", "sparkling", "shiny", "warm", "heroic", "friendly", "financially stable", "oh la la oh la la la la", "romantic", "cozy",
			"wonderful", "fantastic", "super", "great", "delightful", "fabulous", "marvelous", "nice", "pleasant", "good",
			"special", "unique", "o la la la yes yes", "yeah yeah wow wow", "wooooow", "oh woooow", "oh my gawd"
		});
		intensifiers.AddRange(new string[77]
		{
			"totally", "super", "uber", "mega", "super mega", "seriously", "majorly", "legit", "absolutely", "completely",
			"for reals", "utterly", "high-key", "incredibly", "madly", "like ...", "like... seriously", "sooo", "really", "so",
			"sooooooooo", "unbelievably", "very", "like soooo much", "extra", "extremely", "really really", "fiercely", "like, for reals", "greatly",
			"hugely", "immensely", "intensely", "massively", "so so soooo", "really really really", "like totes", "like... totally", "like.. somehow sooo much", "simply",
			"supremely", "surprisingly", "super mega ultra", "ultra", "unusually", "way way", "way", "insanely", "like ... insanelyyyy", "freakishly",
			"extra extra", "overwhelmingly", "reeeaaally", "weirdly", "suuuuper", "way waaaay", "crazy", "like suuuuuper", "really sooo", "low-key",
			"high-key", "literally", "for reeeaal soo", "legit", "honestly", "kinda", "sort of", "basically", "downright", "like literally",
			"very very", "like like.. like... sooo much", "like... actually", "suuuuper suuuuper", "genuinely", "truly", "sincerely"
		});
		nouns.AddRange(new string[108]
		{
			"bae", "bro", "fam", "goals", "friendo", "buddy", "pal", "champ", "MVP", "rockstar",
			"hero", "idol", "star", "queen", "king", "baby", "beast", "boss", "bruh", "boss queen",
			"girl", "dude", "genius", "guru", "cutie", "legend", "player", "boss king", "pog", "partner",
			"prodigy", "pro", "role model", "boy", "soulmate", "superhero", "sweetie", "twin", "robot", "wizard",
			"wonder", "crushie", "angel", "viral hit", "blessing", "champion", "charmer", "crush", "darling", "dear",
			"gamer", "fave", "friend", "gem", "heartthrob", "honey", "heartbreaker", "inspiration", "love", "main",
			"other half", "crushcrush", "person", "precious", "sunshine", "treasure", "bestie", "boo", "cutie", "sister",
			"sis", "brother", "fam", "beauty", "megacrush", "best friend", "supercrush", "favfav", "main character", "robo",
			"icon", "legend", "mood", "vibe", "goat", "man", "woman", "goal", "winner", "yas queen",
			"cute robot", "robot crush", "pal", "sweetheart", "pumpkin", "sugar", "baby", "peach", "dove", "cupcake",
			"buttercup", "snugglebug", "silly goose", "teddy bear", "dream", "princess", "prince", "superstar"
		});
		adverbs.AddRange(new string[77]
		{
			"totally", "super", "uber", "mega", "super mega", "seriously", "majorly", "legit", "absolutely", "completely",
			"for reals", "utterly", "high-key", "incredibly", "madly", "like ...", "like... seriously", "sooo", "really", "so",
			"sooooooooo", "unbelievably", "very", "like soooo", "extra", "extremely", "really really", "fiercely", "like, for reals", "greatly",
			"hugely", "immensely", "intensely", "massively", "so so soooo", "really really really", "like totes", "like... totally", "like.. somehow sooo", "simply",
			"supremely", "surprisingly", "super mega ultra", "ultra", "unusually", "way way", "way", "insanely", "like ... insanelyyyy", "freakishly",
			"extra extra", "overwhelmingly", "reeeaaally", "weirdly", "suuuuper", "way waaaay", "crazy", "like suuuuuper", "really sooo", "low-key",
			"high-key", "literally", "for reeeaal soo", "legit", "honestly", "kinda", "sort of", "basically", "downright", "like literally",
			"very very", "like like.. like... sooo", "like... actually", "suuuuper suuuuper", "genuinely", "truly", "sincerely"
		});
	}

	private string GenerateAffectionateSentence()
	{
		List<string> list = new List<string>
		{
			"Can't even with how {adjective} {playerName} is.", "{playerName} makes everything {intensifier} legit.", "Why is {playerName} so {adjective}? So cute!", "Every time I see {playerName}, I {intransitiveVerb}.", "{playerName} is just {intensifier} {adjective}, you know?", "Got me {adverb} thinking about {playerName} all day.", "Just want to {transitiveVerb} {playerName}.", "Oh my, {playerName} is {intensifier} {adjective}!", "When {playerName} smiles, I {intransitiveVerb}.", "{playerName}, you are so {adjective}!",
			"Can we talk about how {adjective} {playerName} is?", "{playerName} has such a {adjective} vibe.", "Just saw {playerName} looking {adjective}, so sweet.", "Wow, {playerName} is so {adjective}!", "Every time {playerName} talks, I {intransitiveVerb}.", "{playerName} and me = {intensifier} {adjective} vibes.", "Is it just me or is {playerName} {intensifier} {adjective}?", "Not gonna lie, {playerName} is {adverb} {adjective}.", "{playerName} is always {adjective}, and I love it.", "I can't help {intransitiveVerb} over {playerName}.",
			"Guess who has a crush on {playerName}? Me!", "{playerName} walking in makes my day {intensifier} {adjective}.", "Hey {playerName}, keep being you!", "With {playerName}, everything is {adjective}.", "Just {adverb} dreaming about {playerName}.", "{playerName} looks so {adjective} today.", "Low-key, {playerName} is the most {adjective} person.", "High-key crushing on {playerName}!", "{playerName} has that {adjective} something.", "For real, {playerName}'s vibe is {intensifier} {adjective}.",
			"Can't help but {transitiveVerb} {playerName}; they're so {adjective}.", "{playerName} is {adverb} my {noun}!", "Life is more {adjective} with {playerName} around.", "{playerName}'s laugh is {intensifier} {adjective}.", "{playerName}, you {adverb} {transitiveVerb} my world.", "Why is {playerName} so {adjective}?", "Did you see {playerName} today? So {adjective}!", "It's {adverb} {adjective} how much I {transitiveVerb} {playerName}.", "Me, whenever I see {playerName}: So {adjective}!", "{playerName} has me {adverb} {intransitiveVerb}.",
			"Just saw {playerName}, and yep, still {adjective}.", "{playerName} is my {intensifier} {adjective} crush.", "Can confirm, {playerName} is {adjective}!", "Everyday mood: {intransitiveVerb} about how {adjective} {playerName} is.", "{playerName}, stop being so {adjective}; I can't handle it.", "When {playerName} is {intensifier} {adjective}... *swoons*", "Just {intransitiveVerb} about {playerName} being so {adjective}.", "Yep, {playerName} keeps getting more {adjective}.", "{playerName} makes me believe in {intensifier} {adjective} things.", "Daily reminder: {playerName} is {intensifier} {adjective}.",
			"To be honest, {playerName} rocks that {adjective} look {adverb}.", "Seeing {playerName} today was {adverb} the highlight.", "I can't stop {intransitiveVerb} when I think of {playerName}.", "{playerName}, you make my heart {intransitiveVerb}.", "Is it possible to {transitiveVerb} {playerName} more?", "{playerName} is just too {adjective}!", "Thinking about {playerName} makes me {intransitiveVerb}.", "My day gets {adjective} when I see {playerName}.", "{playerName} is my favorite {noun}.", "I {transitiveVerb} {playerName} so much!",
			"Just {adverb} wishing I could {transitiveVerb} {playerName}.", "Whenever I see {playerName}, I {intransitiveVerb} inside.", "{playerName} has the most {adjective} smile.", "Can't wait to {transitiveVerb} {playerName} again.", "If only {playerName} knew how {adjective} they are.", "Feeling {adjective} thanks to {playerName}.", "{playerName}, you're {intensifier} {adjective}!", "I just want to {transitiveVerb} {playerName} all day.", "{playerName}, you make me {intransitiveVerb}.", "Life is {adjective} with {playerName}.",
			"{playerName} is like the most {adjective} dream.", "Can't stop smiling because of {playerName}.", "I think I {transitiveVerb} {playerName}.", "{playerName} makes my heart {intransitiveVerb}.", "Oh, {playerName}, you're so {adjective}!", "Just thinking about {playerName} makes me happy.", "Wish I could {transitiveVerb} {playerName} right now.", "{playerName} is simply {adjective}.", "Feeling {adjective} whenever {playerName} is around.", "{playerName}, you brighten my day!",
			"I {transitiveVerb} {playerName} more than anything.", "Just {adverb} thinking about {playerName}.", "{playerName} is {intensifier} {adjective}!", "Can't get enough of {playerName}'s {adjective} vibes.", "{playerName} is {adverb} {adjective}.", "Just {intransitiveVerb} about how {adjective} {playerName} is.", "{playerName} makes my day {intensifier} awesome."
		};
		string text = list[Random.Range(0, list.Count)];
		string text2 = text.Replace("{playerName}", playerName);
		if (text.Contains("{transitiveVerb}"))
		{
			string newValue = transitiveVerbs[Random.Range(0, transitiveVerbs.Count)];
			text2 = text2.Replace("{transitiveVerb}", newValue);
		}
		if (text.Contains("{intransitiveVerb}"))
		{
			string text3 = intransitiveVerbs[Random.Range(0, intransitiveVerbs.Count)];
			if (text2.Contains("{intransitiveVerb}s"))
			{
				text3 = ((!text3.EndsWith("e")) ? (text3 + "es") : (text3 + "s"));
				text2 = text2.Replace("{intransitiveVerb}s", text3);
			}
			else
			{
				text2 = text2.Replace("{intransitiveVerb}", text3);
			}
		}
		if (text.Contains("{adjective}"))
		{
			string newValue2 = adjectives[Random.Range(0, adjectives.Count)];
			text2 = text2.Replace("{adjective}", newValue2);
		}
		if (text.Contains("{intensifier}"))
		{
			string newValue3 = intensifiers[Random.Range(0, intensifiers.Count)];
			text2 = text2.Replace("{intensifier}", newValue3);
		}
		if (text.Contains("{adverb}"))
		{
			string newValue4 = adverbs[Random.Range(0, adverbs.Count)];
			text2 = text2.Replace("{adverb}", newValue4);
		}
		if (text.Contains("{noun}"))
		{
			string newValue5 = nouns[Random.Range(0, nouns.Count)];
			text2 = text2.Replace("{noun}", newValue5);
		}
		return char.ToUpper(text2[0]) + text2.Substring(1);
	}
}
