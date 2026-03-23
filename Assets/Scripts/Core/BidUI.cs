using UnityEngine;
using System.Collections.Generic;

namespace Core
{
    public class BidUI : MonoBehaviour
    {
        public GameManager gameManager;
        public GameObject bidPanel;
        public List<BidButton> bidButtons;
        int selectedBid = 0;
        

        public void Open()
        {
            bidPanel.SetActive(true);
            for (int i = 0; i < bidButtons.Count; i++)
            {
                int value = i + 1;
                bidButtons[i].Setup(value, OnBidSelected);
            }
        }
        
        void OnBidSelected(int value)
        {
            selectedBid = value;

            foreach (var btn in bidButtons)
            {
                btn.SetSelected(btn.bidValue == value);
            }
        }

        public void SetBid(int bid)
        {
            selectedBid = bid;
            Debug.Log("Selected Bid: " + selectedBid);
        }

        public void ConfirmBid()
        {
            bidPanel.SetActive(false);
            gameManager.OnHumanBidConfirmed(selectedBid);
        }
    }
}