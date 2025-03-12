using BtcWalletLibrary.Models;

using static ElectrumXClient.Response.BlockchainScripthashGetHistoryResponse;
using static ElectrumXClient.Response.BlockchainTransactionGetResponse;
using static ElectrumXClient.Response.BlockchainTransactionGetResponse.BlockchainTransactionGetResult;

namespace BtcWalletLibrary.Tests.Services
{
    public static class MockTransactionData
    {
        public static List<Transaction> TransactionsWithoutConfirmation => _transactionsWithoutConfirmation.Select(t => (Transaction)t.Clone()).ToList();
        private static readonly List<Transaction> _transactionsWithoutConfirmation = [
        new ()
        {
            TransactionHex = "hex2",
            Date = DateTime.Today,
            TransactionId = "00a188329ceb374d5deddd634b7a801f78b5e91dac74ed5b5be49cf84056e535",
            Outputs =
            [
                new TransactionOutput
                {
                    Address = "sampleAddress2",
                    Amount = 0.5,
                    IsUsersAddress = false
                }
            ],
            Inputs =
            [
                new TransactionInput
                {
                    TrId = "previousTxId2",
                    OutputIdx = 2,
                    Address = "inputAddress2",
                    IsUsersAddress = true,
                    Amount = 0.5
                }
            ],
            Confirmed = false
        }];
        public static List<Transaction> TransactionsWithOldDates => _transactionsWithOldDates.Select(t => (Transaction)t.Clone()).ToList();
        private static readonly List<Transaction> _transactionsWithOldDates = [
        new() {
            TransactionHex = "hex1",
            Date = DateTime.Parse("1970-01-01"),
            TransactionId = "8441a9c2c55c3904ced69ba683d1895dd530cd0be6ad63e03bfc9c970270c5c9",
            Outputs =
            [
                new TransactionOutput
                {
                    Address = "sampleAddress1",
                    Amount = 0.2,
                    IsUsersAddress = true
                }
            ],
            Inputs =
            [
                new TransactionInput
                {
                    TrId = "previousTxId1",
                    OutputIdx = 1,
                    Address = "inputAddress1",
                    IsUsersAddress = false,
                    Amount = 0.2
                }
            ],
            Confirmed = false
        }];



        public static List<Transaction> TransactionsWithConfirmationAndUpTodate => _transactionsWithConfirmationAndUpTodate.Select(t => (Transaction)t.Clone()).ToList();
        private static readonly List<Transaction> _transactionsWithConfirmationAndUpTodate = [
        new ()
        {
            TransactionHex = "hex3",
            Date = DateTime.Today,
            TransactionId = "fa86d1f3dae29ddab97934b2dcb907779f624f533ef5641bb82b6933802544df",
            Outputs =
            [
                new () {
                    Address = "sampleAddress3",
                    Amount = 1.0,
                    IsUsersAddress = true
                }
            ],
            Inputs =
            [
                new TransactionInput
                {
                    TrId = "previousTxId3",
                    OutputIdx = 3,
                    Address = "inputAddress3",
                    IsUsersAddress = false,
                    Amount = 1.0
                }
            ],
            Confirmed = true
        }];


        public static BlockchainTransactionGetResult BlockchainTransactionGetResultWithConfrim { get; } = new()
        {
            Blockhash = "blockhash",
            Confirmations = 7,
            Height = 1,
            Hex = "010000000001010811fbe9a5e07a343879a40d591e963a9f479f99422f47a43016958713c77bd61400000000ffffffff0fd06c0400000000002251203dc6ecba5ba26f0955f09af9e0ddd80c9283bbd23180d64fe1234b36184a1ddbd06c040000000000225120e3010e94684a654e00b81f1acc36418cc5130f9be028739769d682d556edf979d06c040000000000225120d41fc405b4af6f40461529212ab4ae567e45224c1360170fd613f92ba6c9c7f0d06c04000000000022512050e75b9d399e53828666b9444ae52922725284cd53cb16158a3ee8f9d4f17202a0f7030000000000225120732b2740cb4d4e1652e823951bf2413a4f235baf8c97661a44d9a6f1ee888da11098020000000000225120dead3928f2648bc3a45ee875e20149decf525c265fcb464df5e061256fa674e5400d0300000000002251201a97e89b05f426547b134298ce98963e86451e5987fcd0cbcf380cfe6153fd54400d0300000000002251202afb830fb9041c00c9a7df8ca00a1ef877559ec5a6aee0f3d9e8e71f25b7d13d7082030000000000225120e1a0ce742b8ace480df71d8e5a2a33d53a9b9699048364e360ca421df36bf9e5a0f7030000000000225120d3ab4cc6ac8a267a4e803e2d04a6c9e3fcfcdea613bb946ba37e8a108b5b8b00a0f7030000000000225120efb45bdc7862c28d4047cb6cecab4f592beb9e72c2a49b5ed162acd14cccb4ce1027000000000000225120e22ad7713b239a54f5f7b085c16ef995bfcb0e609024dd71a41611ef29cf5eeca0f703000000000022512044ec10c18ced08f1e32070cd599937f926f9f53a6698cdad82477e85efdc6871d06c04000000000022512010ce313160b1497295ebacd45f7af2e40e472c701e0f06701ecda515a00f605798a5fb6100000000160014dbd359f23e01f8752cc193fefc04aaa9e3a441400247304402207210835c15b405d0cc022b0d336ea627d00f9ddf6c3f3d5e25890e8682439d0002201cedfc25fdbb28fcb73be9d584933378328bc2fd4744e6491227433e41704e81012102836b1dbc3d40d023ec913ce3d04455a05873ea28e08c6b07536c0f08b3d3d17e00000000",
            Time = 1,
            Txid = TransactionsWithoutConfirmation.First().TransactionId,
            VinValue = [new() { Coinbase = "coinbase", Sequence = 1 }],
            VoutValue = [new() { ScriptPubKey = new ScriptPubKey(), Value = 1 }]
        };

        public static BlockchainTransactionGetResult BlockchainTransactionGetResultWithNewDate { get; } = new()
        {
            Blockhash = "blockhash",
            Confirmations = 7,
            Height = 1,
            Hex = "01000000000101551e1387e0f7268ed7723385aa0e33f81c96b6a5f5b7432e6788a4f789247fc41100000000ffffffff10a0f703000000000022512057d7e33a6d43a333c3b68ab280139a4f4bf6072927c592c83f07aa65e37a1c99c0d4010000000000225120c8b209b5785afb746ed93ee757b712b7af9a725d69bb1a8ea56dc5ca49ff711e400d0300000000002251205bf07be0ecd254219faa2e546da5a82488b56e92f2531cd977b681b8d19e4c4110980200000000002251202bd842cc6aa9b96351207883c65eeee5b4d67fac1c4e6caead987582572742f3400d0300000000002251202d9afc64dc6ded1864a6c247c0698fa7c63f9a93322a5314e3aa6cf2e6db2155d0fb010000000000225120ade01244f898b499465344bd1f9a424d6aa999d4239288d71075c08b0820ed8e1098020000000000225120338e5d8e30c3e3bd899555099386a7441681c9f826cd5079297e2c55de668fcc400d030000000000225120e39ad89ca6cb4291962a571bd812f96028834e7c95e133d2dee037aee18bf104400d03000000000022512089a9470e69abbce6235cb50f9a699240fc39f9621c8f0e0f6bff5b9ca2c49aa4400d0300000000002251209c6e65458e6a66ac16f17668c7adda0e995ae15c9aedd123fb1bc768c8e40efba0f703000000000022512036fe0dc614c7fed30a718f6ebc73f973323dfe92b5ad5bef171869c6de0f5fa2400d030000000000225120e1af067cfe5badaab798cf4a6819d2a5319d7b8f0690219f92b2c9bb7c2a6c77a0f70300000000002251202230ba2ae7ad09f350dda85629a1b8732f5d8e98695844621f6800e0cd07af3aa0f70300000000002251200a2eba649c0450e909810bcbf4e8a7e00f0f204eedbee3f957fe59fd86d586ee400d0300000000002251205bc09fe7fb8444d163d2d703a0bfc7eb9944d2f21497647fc1b706eecb3f6ecbd46dea5100000000160014dbd359f23e01f8752cc193fefc04aaa9e3a441400247304402205c9cf10bb37b2a64cc3f22e5a26104755c4cdeb89d652eac85c38bde6d7b290402206f164a600ae843714cbb353de2f29e604f347c798d4556d048844860c9934b4c012102836b1dbc3d40d023ec913ce3d04455a05873ea28e08c6b07536c0f08b3d3d17e00000000",
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Txid = TransactionsWithOldDates.First().TransactionId,
            VinValue = [new() { Coinbase = "coinbase", Sequence = 1 }],
            VoutValue = [new() { ScriptPubKey = new ScriptPubKey(), Value = 1 }]
        };

        public static BlockchainTransactionGetResult BlockchainTransactionGetResultConfirmedAndUpToDate { get; } = new()
        {
            Blockhash = "blockhash",
            Confirmations = 7,
            Height = 1,
            Hex = "010000000001010000000000000000000000000000000000000000000000000000000000000000ffffffff2c03652537000451f1906704910457210cfc4281672c391600000000000a636b706f6f6c082f636b706f6f6c2fffffffff02c5bb2c0c00000000160014075cdd8fb868692bb05bae69050aa10d564acda00000000000000000266a24aa21a9ed4f65dc47d54417ba91fbea03c808fb06f84b8cd36fe6810dac40f30b0585ea130120000000000000000000000000000000000000000000000000000000000000000000000000",
            Time = 1,
            Txid = TransactionsWithConfirmationAndUpTodate.First().TransactionId,
            VinValue = [new() { Coinbase = "coinbase", Sequence = 1 }],
            VoutValue = [new() { ScriptPubKey = new ScriptPubKey(), Value = 1 }]
        };

        public static BlockchainTransactionGetResult BlockchainTransactionGetResultNewTx { get; } = new()
        {
            Blockhash = "blockhash",
            Confirmations = 1,
            Height = 1,
            Hex = "020000000001017fec20c2328366199969a45358137c67277f52346a4d5af243dc35342849dab40100000000ffffffff03400d03000000000022512073346e50df63c3c034261a8d287eefde45b7bf01c067d02b86f7a60f9b23cb4b0000000000000000356a3359414c410101000101307835374535323836363143413346453566626430353835313839396163636341313830336231666431ca672d00000000002251207ba66d3ae969563e9bc824817d1da34ac9fa01e39b8c466967a7f5b32c48e12f0140534405cea0942a6a0e4a5d116604bf5ef95b42d8bed3a96825f6419045a478a12bb8d11dbbfeae08ed57cc789f7629664e7a30d281cf43ee5aa1e05e80d8777c00000000",
            Time = 1,
            Txid = "fd20215942518bbae615e35673790bb084d15d7287f0560667710671df1e2b57",
            VinValue = [new() { Coinbase = "coinbase", Sequence = 1 }],
            VoutValue = [new() { ScriptPubKey = new ScriptPubKey(), Value = 1 }]
        };

        public static List<BlockchainScripthashGetHistoryResult> BlockchainScripthashGetHistoryResultForTxsWithoutConfirms
        {
            get;
        } = [new() { TxHash = BlockchainTransactionGetResultWithConfrim.Txid }];

        public static List<BlockchainScripthashGetHistoryResult> BlockchainScripthashGetHistoryResultForTxsWithOldDates
        {
            get;
        } = [new() { TxHash = BlockchainTransactionGetResultWithNewDate.Txid }];

        public static List<BlockchainScripthashGetHistoryResult> BlockchainScripthashGetHistoryResultForConfirmedAndUpTodateTxs
        {
            get;
        } = [new() { TxHash = BlockchainTransactionGetResultConfirmedAndUpToDate.Txid }];

        public static List<BlockchainScripthashGetHistoryResult> BlockchainScripthashGetHistoryResultForNewTx { get; } = [new() { TxHash = BlockchainTransactionGetResultNewTx.Txid }];


        public static List<TransactionOutput> TransactionOutputs { get; } =
        [
            new TransactionOutput
            {
                Address = "sampleAddress1",
                Amount = 0.2,
                IsUsersAddress = true
            }
        ];

        public static List<TransactionInput> TransactionInputs { get; } =
        [
            new TransactionInput
            {
                TrId = "previousTxId1",
                OutputIdx = 1,
                Address = "inputAddress1",
                IsUsersAddress = false,
                Amount = 0.2
            }
        ];
    }
}
