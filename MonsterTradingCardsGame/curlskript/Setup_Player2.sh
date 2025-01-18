!/bin/sh

pauseFlag=0
for arg in "$@"; do
    if [ "$arg" == "pause" ]; then
        pauseFlag=1 
        break
    fi
done

if [ $pauseFlag -eq 1 ]; then read -p "Press enter to continue..."; fi

echo "1) Login"
token_altenhof=$(curl -s -X POST http://localhost:10001/api/auth/login --header "Content-Type: application/json" -d "{\"Username\":\"altenhof\", \"Password\":\"markus\"}" | grep -o '"token":"[^"]*' | grep -o '[^"]*$')
echo "$token_altenhof"

if [ $pauseFlag -eq 1 ]; then read -p "Press enter to continue..."; fi

echo "2) list cards of user"

response=$(curl -s -X GET http://localhost:10001/api/cards \
  --header "Content-Type: application/json" \
  --header "Authorization: $token_altenhof")

echo "All cards of user: $response"

#Extract the last 4 IDs using bash tools
ids=$(echo "$response" | grep -o '"Id":"[^"]*"' | tail -4 | awk -F':' '{print $2}' | tr -d '"')

#Prepare the data payload for the second API call
payload="["
for id in $ids; do
    payload+="{\"CardId\":\"$id\"},"
done
payload=${payload%,}] # Remove the trailing comma and close the array

echo "IDs of 4 cards: $payload";

if [ $pauseFlag -eq 1 ]; then read -p "Press enter to continue..."; fi


echo "3) configure deck of user"

#Second API call to update the deck
curl -i -X PUT http://localhost:10001/api/deck \
  --header "Content-Type: application/json" \
  --header "Authorization: $token_altenhof" \
  -d "$payload"

if [ $pauseFlag -eq 1 ]; then read -p "Press enter to continue..."; fi

echo "4) show configured deck of user"

curl -i -X GET http://localhost:10001/api/deck --header "Authorization: $token_altenhof"
