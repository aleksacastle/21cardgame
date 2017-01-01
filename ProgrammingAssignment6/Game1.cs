using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaCards;

namespace ProgrammingAssignment6
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int WindowWidth = 800;
        const int WindowHeight = 600;

        // max valid blockjuck score for a hand
        const int MaxHandValue = 21;

        // deck and hands
        Deck deck;
        List<Card> dealerHand = new List<Card>();
        List<Card> playerHand = new List<Card>();

        // hand placement
        const int TopCardOffset = 100;
        const int HorizontalCardOffset = 150;
        const int VerticalCardSpacing = 125;

        // messages
        SpriteFont messageFont;
        const string ScoreMessagePrefix = "Score: ";
        Message playerScoreMessage;
        Message dealerScoreMessage;
        Message winnerMessage;
		List<Message> messages = new List<Message>();

        // message placement
        const int ScoreMessageTopOffset = 25;
        const int HorizontalMessageOffset = HorizontalCardOffset;
        Vector2 winnerMessageLocation = new Vector2(WindowWidth / 2,
            WindowHeight / 2);

        // menu buttons
        Texture2D quitButtonSprite;
        Texture2D hitButtonSprite;
        Texture2D standButtonSprite;
        MenuButton quitButton;
        MenuButton hitButton;
        MenuButton standButton;
        List<MenuButton> menuButtons = new List<MenuButton>();

        // menu button placement
        const int TopMenuButtonOffset = TopCardOffset;
        const int QuitMenuButtonOffset = WindowHeight - TopCardOffset;
        const int HorizontalMenuButtonOffset = WindowWidth / 2;
        const int VeryicalMenuButtonSpacing = 125;

        // use to detect hand over when player and dealer didn't hit
        bool playerHit = false;
        bool dealerHit = false;

        // game state tracking
        static GameState currentState = GameState.WaitingForPlayer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution and show mouse
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;
            IsMouseVisible = true;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // create and shuffle deck
            deck = new Deck(Content, 0, 0);
            deck.Shuffle();


            // first player card
            Deal(Players.player, true);


            // first dealer card
            Deal(Players.dealer, false);

            // second player card
            Deal(Players.player, true);

            // second dealer card
            Deal(Players.dealer, true);

            // load sprite font, create message for player score and add to list
            messageFont = Content.Load<SpriteFont>(@"bin\fonts\Arial24");
            playerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(playerHand).ToString(),
                messageFont,
                new Vector2(HorizontalMessageOffset, ScoreMessageTopOffset));
            messages.Add(playerScoreMessage);

            // load quit button sprite for later use
			quitButtonSprite = Content.Load<Texture2D>(@"graphics\quitbutton");

            // create hit button and add to list
            MenuButton hitButton = new MenuButton(Content.Load<Texture2D>(@"graphics\hitbutton"),
                                    new Vector2(WindowWidth / 2, TopCardOffset),
                                    GameState.PlayerHitting);
            menuButtons.Add(hitButton);


            // create stand button and add to list
            MenuButton standButton = new MenuButton(Content.Load<Texture2D>(@"graphics\standbutton"),
                                    new Vector2(WindowWidth / 2, TopCardOffset + VerticalCardSpacing),
                                    GameState.WaitingForDealer);
            menuButtons.Add(standButton);


        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            MouseState mouse = Mouse.GetState();

            // update menu buttons as appropriate
            if (currentState == GameState.WaitingForPlayer ||
                currentState == GameState.DisplayingHandResults)
            {
                foreach (MenuButton menubutton in menuButtons)
                {
                    menubutton.Update(mouse);
                }
            }


            // game state-specific processing

            switch (currentState)
            {
                //if player hitting
                case GameState.PlayerHitting:
                    Deal(Players.player, true);
                    playerScoreMessage.Text = ScoreMessagePrefix + GetBlockjuckScore(playerHand).ToString();
                    currentState = GameState.WaitingForDealer;
                    playerHit = true;
                    break;
                //deciding whether dealer hitting or not
                case GameState.WaitingForDealer:
                    if (GetBlockjuckScore(dealerHand) < 17)
                    {
                        currentState = GameState.DealerHitting;
                    }
                    else
                    {
                        currentState = GameState.CheckingHandOver;
                    }
                    break;
                //if dealer hitting
                case GameState.DealerHitting:
                    Deal(Players.dealer, true);
                    currentState = GameState.CheckingHandOver;
                    dealerHit = true;
                    break;

                case GameState.CheckingHandOver:

                    //checking if busted
                    if ((playerHit &&
                        GetBlockjuckScore(playerHand) > MaxHandValue) ||
                        (dealerHit &&
                        GetBlockjuckScore(dealerHand) > MaxHandValue))

                    {

                        string winString;

                        //check if a tie
                        if (playerHit &&
                            GetBlockjuckScore(playerHand) > MaxHandValue &&
                            dealerHit &&
                            GetBlockjuckScore(dealerHand) > MaxHandValue)
                        {
                            winString = "It's a tie";

                        }
                        //check if player wins
                        else if (GetBlockjuckScore(playerHand) > MaxHandValue)
                        {
                            winString = "Dealer wins";
                        }
                        else
                        {
                            winString = "Player wins";
                        }
                        Results(winString);

                    }

                    else
                    {
                        //if both stand
                        if (!playerHit &&
                            !dealerHit)
                        {

                            string winString;
                            //check for tie
                            if (GetBlockjuckScore(dealerHand) == GetBlockjuckScore(playerHand))
                            {
                                winString = "It's a tie";
                            }
                            //player wins
                            else if (GetBlockjuckScore(playerHand) > GetBlockjuckScore(dealerHand))
                            {
                                winString = "Player wins";

                            }
                            //dealer wins
                            else
                            {
                                winString = "Dealer wins";

                            }
                            Results(winString);
                        }
                        currentState = GameState.WaitingForPlayer;
                        playerHit = false;
                        dealerHit = false;
                    }
                    break;

                //exit game
                case GameState.Exiting:
                    this.Exit();
                    break;
                default:
                    break;
            
            }

        

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Goldenrod);
						
            spriteBatch.Begin();

            // draw hands
            foreach (Card cards in playerHand)
            {
                cards.Draw(spriteBatch);
            }

            foreach (Card card in dealerHand)
            {
                card.Draw(spriteBatch);
            }

            // draw messages
            foreach(Message message in messages)
            {
                message.Draw(spriteBatch);
            }


            // draw menu buttons
            foreach(MenuButton menubutton in menuButtons)
            {
                menubutton.Draw(spriteBatch);
            }
           

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Calculates the Blockjuck score for the given hand
        /// </summary>
        /// <param name="hand">the hand</param>
        /// <returns>the Blockjuck score for the hand</returns>
        private int GetBlockjuckScore(List<Card> hand)
        {
            // add up score excluding Aces
            int numAces = 0;
            int score = 0;
            foreach (Card card in hand)
            {
                if (card.Rank != Rank.Ace)
                {
                    score += GetBlockjuckCardValue(card);
                }
                else
                {
                    numAces++;
                }
            }

            // if more than one ace, only one should ever be counted as 11
            if (numAces > 1)
            {
                // make all but the first ace count as 1
                score += numAces - 1;
                numAces = 1;
            }

            // if there's an Ace, score it the best way possible
            if (numAces > 0)
            {
                if (score + 11 <= MaxHandValue)
                {
                    // counting Ace as 11 doesn't bust
                    score += 11;
                }
                else
                {
                    // count Ace as 1
                    score++;
                }
            }

            return score;
        }

        /// <summary>
        /// Gets the Blockjuck value for the given card
        /// </summary>
        /// <param name="card">the card</param>
        /// <returns>the Blockjuck value for the card</returns>
        private int GetBlockjuckCardValue(Card card)
        {
            switch (card.Rank)
            {
                case Rank.Ace:
                    return 11;
                case Rank.King:
                case Rank.Queen:
                case Rank.Jack:
                case Rank.Ten:
                    return 10;
                case Rank.Nine:
                    return 9;
                case Rank.Eight:
                    return 8;
                case Rank.Seven:
                    return 7;
                case Rank.Six:
                    return 6;
                case Rank.Five:
                    return 5;
                case Rank.Four:
                    return 4;
                case Rank.Three:
                    return 3;
                case Rank.Two:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Changes the state of the game
        /// </summary>
        /// <param name="newState">the new game state</param>
        public static void ChangeState(GameState newState)
        {
            currentState = newState;
        }
       
        /// <summary>
        /// Gets appropriate winning message
        /// </summary>
        /// <param name="winString">gets appropriate message</param>
        private void Results(string winString)
        {
            //remove hit and stand buttons
            menuButtons.Clear();


            // create hit button and add to list
            MenuButton quitButton = new MenuButton(quitButtonSprite,
                                                    new Vector2(WindowWidth / 2, QuitMenuButtonOffset),
                                                    GameState.Exiting);
            menuButtons.Add(quitButton);

            //show first card of dealer and calculate his score. Add message to the messages list
            dealerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString(),
                messageFont,
                new Vector2(WindowWidth - HorizontalMessageOffset, ScoreMessageTopOffset));
            messages.Add(dealerScoreMessage);
            dealerHand[0].FlipOver();

            //write appropariate message
            Message winMessage = new Message(winString, messageFont, winnerMessageLocation);
            messages.Add(winMessage);

            currentState = GameState.DisplayingHandResults;
        }
        
        /// <summary>
        /// deal card
        /// </summary>
        /// <param name="player">player or dealer</param>
        /// <param name="faceup">flip over or not</param>
        private void Deal(Players player,  bool faceup)
            {

                List<Card> hand = new List<Card>();

                //take card from the top of the deck
                Card card = deck.TakeTopCard();

                //flip card
                if (faceup)
                {
                     card.FlipOver();
                }

                //give card to player or dealer
               if (player == Players.player)
                 {
                hand = playerHand;
                card.X = HorizontalCardOffset;
                    }
                if (player == Players.dealer)
                {
                hand = dealerHand;
                card.X = WindowWidth - HorizontalCardOffset;
                }
                 card.Y = TopCardOffset + (hand.Count * VerticalCardSpacing);
            
            hand.Add(card);
           
        }
        /// <summary>
        /// define type of player
        /// </summary>
        public enum Players
        {
            player,
            dealer
        }
    }
}
