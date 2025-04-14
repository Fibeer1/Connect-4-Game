using UnityEngine;

public class GridColumn : Interactable
{   
    [SerializeField] private GameObject tokenPreviewPrefab;
    private GameObject tokenPreviewInstance;
    [SerializeField] private Transform tokenPreviewPosition;
    [SerializeField] private int columnIndex;
    private TokenGrid tokenGrid;
    private RoundManager roundManager;

    private void Start()
    {
        //1 instance of the grid and round manager will be available in the scene, so might as well make referencing automatic
        tokenGrid = FindAnyObjectByType<TokenGrid>();
        roundManager = FindAnyObjectByType<RoundManager>();
    }

    public override void OnHoverEnter()
    {
        base.OnHoverEnter();
        //Do not spawn a preview if it's not the player's turn or if the column is full
        if (tokenPreviewInstance != null || 
            roundManager.currentPlayerTurn != RoundManager.PlayerType.Player || 
            tokenGrid.IsColumnFull(columnIndex))
        {
            return;
        }
        if (tokenPreviewInstance == null)
        {
            tokenPreviewInstance = Instantiate(tokenPreviewPrefab, tokenPreviewPosition.position, Quaternion.identity, transform);
        }
    }

    public override void OnHoverExit()
    {
        base.OnHoverExit();
        //If the preview has been spawned, then it's the player's turn,
        //meaning there's no need to check if it is
        DestroyPreview();
    }

    private void DestroyPreview()
    {
        if (tokenPreviewInstance != null)
        {
            Destroy(tokenPreviewInstance);
            tokenPreviewInstance = null;
        }
    }

    public override void Interact()
    {
        base.Interact();
        tokenGrid.AddTokenToColumn(columnIndex, 1);
        DestroyPreview();
        roundManager.SwitchTurn();
    }
}
